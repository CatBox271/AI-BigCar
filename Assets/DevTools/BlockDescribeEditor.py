# -*- coding: utf-8 -*-
"""方块编辑器（仅开发期使用）

- 编辑对象：Assets/Resources/Data/BlockLibrary.json 的 chinese_name（中文名）、
  describe（描述）、adjudication（判词）
- 代号只读：cards.json 的 block_kind 存的就是这里的下标，条目和顺序都不能动
- 左边是带图标的方块墙，图来自 Assets/Resources/Sprite/<代号>.png（大小写不敏感）
- 描述里用的是 TextMeshPro 富文本，<color> 和 </color> 必须成对

用法：
    python Assets/DevTools/BlockDescribeEditor.py            # 打开窗口
    python Assets/DevTools/BlockDescribeEditor.py --selftest # 只跑数据自检
    python Assets/DevTools/BlockDescribeEditor.py --smoke    # 建窗口后自动关闭
"""

import json
import os
import re
import sys
import tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent


def find_project_root():
    """从脚本位置往上找，认同时有 ProjectSettings 和 Assets 的目录为工程根。"""
    for p in [HERE] + list(HERE.parents):
        if (p / "Assets").is_dir() and (p / "ProjectSettings").is_dir():
            return p
    return HERE.parent


PROJECT_ROOT = find_project_root()
DATA_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Data"
BLOCKLIB_PATH = DATA_DIR / "BlockLibrary.json"
SPRITE_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Sprite"

# 与 GameManager.BlockInfo 的字段一一对应
BLOCK_KEYS = ["code_name", "chinese_name", "describe", "adjudication"]

# 现有描述都在用的三段标签，点一下插到光标处
TAG_PRESETS = [
    ("作用", "<color=#FFFF80FF>作用</color>："),
    ("属性", "<color=#FFCB31FF>属性</color>："),
    ("特性", "<color=orange>特性</color>："),
]


# ---------------------------------------------------------------- 数据层

def load_blocks():
    with open(BLOCKLIB_PATH, encoding="utf-8") as f:
        doc = json.load(f)
    return [{k: raw.get(k, "") for k in BLOCK_KEYS} for raw in (doc.get("blocks") or [])]


def save_blocks(blocks):
    """按固定字段顺序、UTF-8 无 BOM、LF 写盘；先写临时文件再替换。"""
    doc = {"blocks": [{k: b[k] for k in BLOCK_KEYS} for b in blocks]}
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    fd, tmp = tempfile.mkstemp(dir=str(DATA_DIR), suffix=".tmp")
    os.close(fd)
    try:
        with open(tmp, "w", encoding="utf-8", newline="\n") as f:
            json.dump(doc, f, ensure_ascii=False, indent=2)
            f.write("\n")
        os.replace(tmp, BLOCKLIB_PATH)
    finally:
        if os.path.exists(tmp):
            os.remove(tmp)


def sprite_index():
    """代号(小写) -> png 路径，兼容 Engine/engine.png 这种大小写不一致。"""
    idx = {}
    if SPRITE_DIR.is_dir():
        for p in SPRITE_DIR.iterdir():
            if p.suffix.lower() == ".png":
                idx.setdefault(p.stem.lower(), p)
    return idx


def tag_problems(text):
    """<color> 没配对的描述，返回问题描述列表。"""
    opens = len(re.findall(r"<color[^>]*>", text))
    closes = text.count("</color>")
    if opens != closes:
        return ["<color> %d 个，</color> %d 个" % (opens, closes)]
    return []


def validate(blocks):
    """返回错误描述列表，空列表代表可以保存。"""
    errs = []
    seen = {}
    for i, b in enumerate(blocks):
        code = b.get("code_name", "")
        name = b.get("chinese_name", "").strip()
        if not code:
            errs.append("第 %d 条：代号不能为空" % (i + 1))
        if not name:
            errs.append("%s：中文名不能为空" % (code or ("第 %d 条" % (i + 1))))
        elif name in seen:
            errs.append("%s：中文名和第 %d 条重复" % (name, seen[name] + 1))
        else:
            seen[name] = i
        for problem in tag_problems(b.get("describe", "")):
            errs.append("%s：描述里 %s" % (name or code, problem))
    return errs


def selftest():
    blocks = load_blocks()
    print("方块表：%d 条" % len(blocks))
    print("字段：%s" % ", ".join(BLOCK_KEYS))
    errs = validate(blocks)
    if errs:
        print("校验失败：")
        for e in errs:
            print("  - " + e)
        return 1
    print("校验通过")

    idx = sprite_index()
    no_icon = [b["code_name"] for b in blocks if b["code_name"].lower() not in idx]
    print("图标：%d 个，无图方块 %s" % (len(idx), no_icon or "无"))

    assert tag_problems("<color=red>a</color>") == [], "正常标签被误报"
    assert tag_problems("<color=red>a") != [], "少闭合没查出来"
    assert tag_problems("</color>a") != [], "多闭合没查出来"
    assert "中文名不能为空" in "".join(validate([{"code_name": "X", "chinese_name": " ",
                                                 "describe": "", "adjudication": ""}])), "空中文名没查出来"
    print("标签与中文名检查正常")

    before = json.dumps(blocks, ensure_ascii=False, sort_keys=True)
    fd, tmp = tempfile.mkstemp(suffix=".json")
    os.close(fd)
    try:
        doc = {"blocks": [{k: b[k] for k in BLOCK_KEYS} for b in blocks]}
        with open(tmp, "w", encoding="utf-8", newline="\n") as f:
            json.dump(doc, f, ensure_ascii=False, indent=2)
        with open(tmp, encoding="utf-8") as f:
            back = json.load(f)["blocks"]
        assert before == json.dumps(back, ensure_ascii=False, sort_keys=True), "写盘再回读不一致"
        print("读写往返一致")
    finally:
        os.remove(tmp)
    print("SELFTEST PASS")
    return 0


# ---------------------------------------------------------------- 图标

try:
    from PIL import Image, ImageTk
    HAS_PIL = True
except Exception:                                    # 没装 Pillow 就只显示文字
    HAS_PIL = False

import tkinter as tk
from tkinter import ttk, messagebox

UI_FONT = ("Microsoft YaHei UI", 9)
UI_FONT_S = ("Microsoft YaHei UI", 8)
UI_FONT_B = ("Microsoft YaHei UI", 10, "bold")
UI_FONT_TEXT = ("Microsoft YaHei UI", 10)

WALL_BG = "#3c3c3c"
TILE_BG = "#4a4a4a"
TILE_BORDER = "#7a7a7a"
TILE_SELECTED = "#2f8fff"
TILE_TEXT = "#f0f0f0"
TILE_W, TILE_H = 136, 152


class IconCache:
    """把 Sprite/<代号>.png 缩成指定尺寸的 PhotoImage，按 (代号,尺寸) 缓存。"""

    def __init__(self):
        self.cache = {}
        self.index = sprite_index()

    def get(self, code, size):
        key = ((code or "").lower(), size)
        if key in self.cache:
            return self.cache[key]
        img = None
        path = self.index.get(key[0]) if code else None
        if HAS_PIL and path is not None:
            try:
                im = Image.open(path).convert("RGBA")
                im.thumbnail((size, size), Image.LANCZOS)
                pad = Image.new("RGBA", (size, size), (0, 0, 0, 0))
                pad.alpha_composite(im, ((size - im.width) // 2, (size - im.height) // 2))
                img = ImageTk.PhotoImage(pad)
            except Exception:
                img = None
        self.cache[key] = img
        return img


# ---------------------------------------------------------------- 界面

class App:
    def __init__(self, root):
        self.root = root
        self.blocks = load_blocks()
        self.sel = None                 # 正在编辑的 BlockLibrary 下标
        self.dirty = False
        self.icons = IconCache()
        self.tiles = {}
        self.cols_now = 0

        root.title("方块编辑器 — %s" % BLOCKLIB_PATH.name)
        root.geometry("1240x780")
        root.minsize(900, 560)

        self._build_toolbar()
        self._build_body()
        self.root.update_idletasks()
        self.refresh_wall()
        if self.blocks:
            self.select(0)
        self.update_status()

    # ---------------- 骨架
    def _build_toolbar(self):
        bar = ttk.Frame(self.root, padding=(8, 6))
        bar.pack(side="top", fill="x")

        def btn(text, cmd, width):
            ttk.Button(bar, text=text, command=cmd, width=width).pack(side="left", padx=2)

        btn("保存", self.save, 7)
        btn("重新载入", self.reload, 9)
        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=8)
        btn("打开数据目录", self.open_folder, 12)

        ttk.Label(bar, text="筛选").pack(side="left", padx=(14, 2))
        self.filter_var = tk.StringVar()
        ttk.Entry(bar, textvariable=self.filter_var, width=16).pack(side="left")
        self.filter_var.trace_add("write", lambda *a: self.refresh_wall())

    def _build_body(self):
        self.status = ttk.Label(self.root, text="", anchor="w", font=UI_FONT)
        self.status.pack(side="bottom", fill="x", padx=10, pady=(0, 6))

        paned = ttk.PanedWindow(self.root, orient="horizontal")
        paned.pack(side="top", fill="both", expand=True, padx=8, pady=(0, 4))

        left = ttk.Frame(paned)
        paned.add(left, weight=3)
        self.canvas = tk.Canvas(left, background=WALL_BG, highlightthickness=0)
        sb = ttk.Scrollbar(left, orient="vertical", command=self.canvas.yview)
        self.canvas.configure(yscrollcommand=sb.set)
        self.canvas.pack(side="left", fill="both", expand=True)
        sb.pack(side="right", fill="y")
        self.wall = tk.Frame(self.canvas, background=WALL_BG)
        self.canvas.create_window((0, 0), window=self.wall, anchor="nw", tags="wall")
        self.canvas.bind("<Configure>", self._on_canvas_configure)
        self.canvas.bind("<Button-1>", lambda e: None)
        self.canvas.bind("<MouseWheel>", self._on_wheel)

        right = ttk.Frame(paned)
        paned.add(right, weight=2)
        box = ttk.Frame(right, padding=(12, 10))
        box.pack(fill="both", expand=True)

        head = ttk.Frame(box)
        head.pack(fill="x")
        self.icon_box = tk.Canvas(head, width=128, height=128, background=WALL_BG, highlightthickness=0)
        self.icon_box.pack(side="left", padx=(0, 12))
        info = ttk.Frame(head)
        info.pack(side="left", fill="both", expand=True)

        self.title_var = tk.StringVar()
        ttk.Label(info, textvariable=self.title_var, font=UI_FONT_B).pack(anchor="w")
        self.code_var = tk.StringVar()
        ttk.Label(info, textvariable=self.code_var, font=UI_FONT_S, foreground="#888").pack(anchor="w", pady=(2, 10))

        ttk.Label(info, text="中文名", font=UI_FONT).pack(anchor="w")
        self.cname_var = tk.StringVar()
        entry = ttk.Entry(info, textvariable=self.cname_var)
        entry.pack(fill="x", pady=(2, 0))
        entry.bind("<KeyRelease>", lambda e: self.apply_live())
        ttk.Label(info, text="改名即时反映到左边方块墙；卡片有自己的 card_name，不会跟着变",
                  foreground="#888", font=UI_FONT_S, wraplength=300).pack(anchor="w", pady=(3, 0))

        ttk.Label(box, text="描述", font=UI_FONT).pack(anchor="w", pady=(12, 2))
        dbox = ttk.Frame(box)
        dbox.pack(fill="both", expand=True)
        self.t_desc = tk.Text(dbox, height=12, width=1, wrap="word", font=UI_FONT_TEXT, undo=True)
        self.t_desc.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(dbox, orient="vertical", command=self.t_desc.yview).pack(side="right", fill="y")

        tags = ttk.Frame(box)
        tags.pack(fill="x", pady=(4, 10))
        ttk.Label(tags, text="插入", font=UI_FONT_S, foreground="#888").pack(side="left", padx=(0, 6))
        for text, snippet in TAG_PRESETS:
            ttk.Button(tags, text=text, width=5,
                       command=lambda s=snippet: self.insert_tag(s)).pack(side="left", padx=2)

        ttk.Label(box, text="判词", font=UI_FONT).pack(anchor="w")
        abox = ttk.Frame(box)
        abox.pack(fill="x", pady=(2, 4))
        self.t_adj = tk.Text(abox, height=3, width=1, wrap="word", font=UI_FONT_TEXT, undo=True)
        self.t_adj.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(abox, orient="vertical", command=self.t_adj.yview).pack(side="right", fill="y")

        self.tip = ttk.Label(box, text="", font=UI_FONT_S, foreground="#888")
        self.tip.pack(anchor="w")

        self.t_desc.bind("<KeyRelease>", lambda e: self.apply_live())
        self.t_adj.bind("<KeyRelease>", lambda e: self.apply_live())

    def _on_canvas_configure(self, event):
        self.canvas.itemconfigure("wall", width=event.width)
        cols = max(1, (event.width - 12) // (TILE_W + 12))
        if cols != self.cols_now:
            self.cols_now = cols
            self.refresh_wall()

    def _cols(self):
        """按画布实际宽度算列数；窗口还没画出来时先按 4 列显。"""
        w = self.canvas.winfo_width()
        if w <= 1: return 4
        return max(1, (w - 12) // (TILE_W + 12))

    def _on_wheel(self, event):
        self.canvas.yview_scroll(int(-event.delta / 120), "units")

    # ---------------- 方块墙
    def _match(self, b):
        kw = self.filter_var.get().strip().lower()
        if not kw:
            return True
        text = "%s %s" % (b.get("chinese_name", ""), b.get("code_name", ""))
        return kw in text.lower()

    def refresh_wall(self, keep_scroll=True):
        y = self.canvas.yview()[0] if keep_scroll else 0.0
        for w in self.wall.winfo_children():
            w.destroy()
        self.tiles.clear()

        cols = self._cols()
        row = col = 0
        for i, b in enumerate(self.blocks):
            if not self._match(b):
                continue
            self._make_tile(i, b).grid(row=row, column=col, padx=6, pady=6, sticky="nw")
            col += 1
            if col >= cols:
                col, row = 0, row + 1
        self.wall.update_idletasks()
        self.canvas.configure(scrollregion=self.canvas.bbox("all"))
        self.canvas.yview_moveto(y)
        self._highlight()

    def _make_tile(self, i, b):
        code = b.get("code_name", "")
        frame = tk.Frame(self.wall, width=TILE_W, height=TILE_H, bg=TILE_BG,
                         highlightthickness=3, highlightbackground=TILE_BORDER)
        frame.grid_propagate(False)
        frame.pack_propagate(False)

        img = self.icons.get(code, 84)
        icon = tk.Label(frame, bg=TILE_BG, fg="#8a8a8a", font=UI_FONT_S,
                        image=img or "", text="" if img else "无图")
        icon.image = img
        icon.pack(side="top", expand=True, pady=(6, 0))

        code_label = tk.Label(frame, text="%d  %s" % (i, code or "?"), font=UI_FONT_S,
                              bg=TILE_BG, fg="#a0a0a0")
        code_label.pack(side="bottom", pady=(0, 4))
        name = tk.Label(frame, text=b.get("chinese_name") or "未命名", font=UI_FONT_B,
                        bg=TILE_BG, fg=TILE_TEXT, wraplength=TILE_W - 18, justify="center")
        name.pack(side="bottom", pady=(0, 2))

        for w in (frame, icon, name, code_label):
            w.bind("<Button-1>", lambda e, idx=i: self.select(idx))
            w.bind("<MouseWheel>", self._on_wheel)
        self.tiles[i] = {"frame": frame, "name": name, "icon": icon}
        return frame

    def _highlight(self):
        for i, t in self.tiles.items():
            t["frame"].configure(highlightbackground=TILE_SELECTED if i == self.sel else TILE_BORDER)

    def sync_tile(self, i=None):
        """把数据同步到墙上的那块方块。"""
        if i is None:
            i = self.sel
        t = self.tiles.get(i)
        if t is None or i >= len(self.blocks):
            return
        t["name"].configure(text=self.blocks[i].get("chinese_name") or "未命名")

    # ---------------- 选择与表单
    def select(self, i):
        """选中下标为 i 的方块（下标与 BlockLibrary 一致）。"""
        if i == self.sel or not 0 <= i < len(self.blocks):
            return
        self.flush()
        self.sel = i
        self.load_form()
        self._highlight()
        self.update_status()

    def load_form(self):
        b = self.blocks[self.sel]
        self.title_var.set(b.get("chinese_name") or "未命名")
        self.code_var.set("代号 %s   下标 %d" % (b.get("code_name") or "?", self.sel))
        self.cname_var.set(b.get("chinese_name", ""))
        self._set_text(self.t_desc, b.get("describe", ""))
        self._set_text(self.t_adj, b.get("adjudication", ""))
        self.update_icon()
        self.update_tip()

    def update_icon(self):
        img = self.icons.get(self.blocks[self.sel].get("code_name", ""), 128)
        self.icon_box.delete("all")
        if img:
            self.icon_box.create_image(64, 64, image=img, anchor="center")
        else:
            self.icon_box.create_text(64, 64, text="无图", fill=TILE_TEXT, font=UI_FONT_S)
        self.icon_box.image = img

    @staticmethod
    def _set_text(widget, value):
        widget.delete("1.0", "end")
        widget.insert("1.0", value or "")

    def apply_live(self):
        """输入先落回数据，再刷新墙和提示，避免拿旧值刷新。"""
        self.flush()
        self.sync_tile()
        self.update_tip()

    def flush(self):
        if self.sel is None or self.sel >= len(self.blocks):
            return
        b = self.blocks[self.sel]
        name = self.cname_var.get().strip()
        desc = self.t_desc.get("1.0", "end-1c")
        adj = self.t_adj.get("1.0", "end-1c")
        if (b.get("chinese_name"), b.get("describe"), b.get("adjudication")) == (name, desc, adj):
            return
        b["chinese_name"] = name
        b["describe"] = desc
        b["adjudication"] = adj
        self.dirty = True
        self.title_var.set(name or "未命名")
        self.update_status()

    def update_tip(self):
        text = self.t_desc.get("1.0", "end-1c")
        tip = "描述 %d 字" % len(text)
        problems = tag_problems(text)
        if problems:
            tip += "  |  标签没配对：" + "；".join(problems)
        self.tip.configure(text=tip)

    def insert_tag(self, snippet):
        self.t_desc.insert("insert", snippet)
        self.t_desc.focus_set()
        self.apply_live()

    # ---------------- 命令
    def save(self):
        self.flush()
        errs = validate(self.blocks)
        if errs:
            messagebox.showwarning("无法保存", "请先修正以下问题：\n\n" + "\n".join(errs[:12]) +
                                   ("\n…" if len(errs) > 12 else ""))
            self.update_status("校验失败 %d 项" % len(errs))
            return False
        try:
            save_blocks(self.blocks)
        except Exception as exc:
            messagebox.showerror("保存失败", str(exc))
            return False
        self.dirty = False
        self.update_status("已保存 %s（%d 条）" % (BLOCKLIB_PATH.name, len(self.blocks)))
        return True

    def reload(self):
        if self.dirty and not messagebox.askyesno("重新载入", "有未保存的改动，确定丢弃？"):
            return
        self.blocks = load_blocks()
        self.icons = IconCache()
        self.sel = None
        self.dirty = False
        self.refresh_wall()
        if self.blocks:
            self.select(0)
        self.update_status("已重新载入（%d 条）" % len(self.blocks))

    def open_folder(self):
        try:
            os.startfile(str(DATA_DIR))
        except Exception as exc:
            messagebox.showerror("打不开目录", str(exc))

    def update_status(self, extra=""):
        mark = "● 未保存" if self.dirty else "已保存"
        text = "方块 %d 条 | %s" % (len(self.blocks), mark)
        if extra:
            text += " | " + extra
        self.status.configure(text=text)

    def on_close(self):
        if self.dirty:
            ans = messagebox.askyesnocancel("退出", "有未保存的改动，退出前保存吗？")
            if ans is None:
                return
            if ans and not self.save():
                return
        self.root.destroy()


def main():
    args = sys.argv[1:]
    if "--selftest" in args:
        return selftest()
    root = tk.Tk()
    app = App(root)
    if "--smoke" in args:
        root.update_idletasks()
        root.update()
        app.select(3)
        app.cname_var.set(app.cname_var.get())
        root.update_idletasks()
        print("UI OK：方块 %d 条，墙上 %d 块，当前下标 %d，有图 %d 个"
              % (len(app.blocks), len(app.tiles), app.sel,
                 sum(1 for t in app.tiles.values() if t["icon"].image is not None)))
        root.destroy()
        return 0
    root.protocol("WM_DELETE_WINDOW", app.on_close)
    root.mainloop()
    return 0


if __name__ == "__main__":
    sys.exit(main())
