# -*- coding: utf-8 -*-
"""卡片预设编辑器（仅开发期使用）

- 数据源：Assets/Resources/Data/BlockLibrary.json（方块表，只读）
- 编辑对象：Assets/Resources/Data/cards.json（卡片预设，可读可写）
- 图标：Assets/Resources/Sprite/<code_name>.png（大小写不敏感）

卡片字段与 CardOb 一一对应：block_kind 存方块库下标（int），名字存 card_name。没有自造的唯一键。

用法：
    python Assets/DevTools/CardPresetEditor.py            # 打开窗口
    python Assets/DevTools/CardPresetEditor.py --selftest # 只跑数据自检
    python Assets/DevTools/CardPresetEditor.py --smoke    # 建窗口后自动关闭
"""

import json
import os
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
CARDS_PATH = DATA_DIR / "cards.json"
SPRITE_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Sprite"

# 与 CardOb 的字段一一对应（没有 id 这种自造键）
CARD_KEYS = ["card_state", "block_kind", "card_name", "describe",
             "adjudication", "cost", "behaviour"]

STATE_NAMES = ["实物牌", "技能牌"]


# ---------------------------------------------------------------- 数据层

def load_blocks():
    """读方块表，返回 (list, {code_name: info})。"""
    with open(BLOCKLIB_PATH, encoding="utf-8") as f:
        doc = json.load(f)
    blocks = doc.get("blocks") or []
    return blocks, {b["code_name"]: b for b in blocks}


def code_names(blocks):
    """方块代号列表，位置就是 cards.json 里 block_kind 存的那个下标。"""
    return [b.get("code_name", "") for b in blocks]


def sprite_index():
    """code_name(小写) -> png 路径，兼容 Engine/engine.png 这种大小写不一致。"""
    idx = {}
    if SPRITE_DIR.is_dir():
        for p in SPRITE_DIR.iterdir():
            if p.suffix.lower() == ".png":
                idx.setdefault(p.stem.lower(), p)
    return idx


def make_card(card_state=0, block_kind=-1, cost=1, card_name=""):
    """block_kind 存 GameManager.BlockLibrary 的下标，-1 表示没选方块。"""
    return {
        "card_state": card_state,
        "block_kind": block_kind,
        "card_name": card_name,         # 生成时默认取方块库里的中文名
        "describe": "",                 # 留空 → 代码回退 BlockLibrary
        "adjudication": "",             # 留空 → 代码回退 BlockLibrary
        "cost": cost,
        "behaviour": [],
    }


def block_card(block, index):
    """按方块表的一条记录生成一张实物卡（下标和中文名直接填上）。"""
    return make_card(block_kind=index, card_name=block.get("chinese_name", ""))


def seed_cards(blocks):
    """默认把方块库里每个方块都建成一张实物卡（跳过占位的 Null）。"""
    return [block_card(b, i) for i, b in enumerate(blocks)
            if b.get("code_name") not in ("", "Null")]


def load_cards(blocks):
    """cards.json 不存在就按方块表生成一份；返回 (cards, created)。"""
    if not CARDS_PATH.exists():
        cards = seed_cards(blocks)
        save_cards(cards)
        return cards, True
    with open(CARDS_PATH, encoding="utf-8") as f:
        doc = json.load(f)
    cards = []
    for raw in doc.get("cards") or []:
        c = make_card()
        for k in CARD_KEYS:
            if k in raw:
                c[k] = raw[k]
        c["block_kind"] = int(c.get("block_kind", -1))
        c["behaviour"] = [str(x) for x in (c.get("behaviour") or [])]
        cards.append(c)
    return cards, False


def save_cards(cards):
    """按固定字段顺序、UTF-8 无 BOM、LF 写盘；先写临时文件再替换。"""
    doc = {"cards": [{k: c[k] for k in CARD_KEYS} for c in cards]}
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    fd, tmp = tempfile.mkstemp(dir=str(DATA_DIR), suffix=".tmp")
    os.close(fd)
    try:
        with open(tmp, "w", encoding="utf-8", newline="\n") as f:
            json.dump(doc, f, ensure_ascii=False, indent=2)
            f.write("\n")
        os.replace(tmp, CARDS_PATH)
    finally:
        if os.path.exists(tmp):
            os.remove(tmp)


def validate(cards, blocks):
    """返回错误描述列表，空列表代表可以保存。"""
    codes = code_names(blocks)
    errs = []
    for i, c in enumerate(cards):
        index = int(c.get("block_kind", -1))
        code = codes[index] if 0 <= index < len(codes) else ""
        name = c.get("card_name") or code or ("第 %d 张" % (i + 1))
        st = int(c.get("card_state", 0)) if int(c.get("card_state", 0)) in (0, 1) else 0
        if st == 0 and index < 0:
            errs.append("%s：实物牌必须选方块种类" % name)
        elif st == 0 and not code:
            errs.append("%s：方块种类下标 %d 超出 BlockLibrary（%d 条）" % (name, index, len(codes)))
        if int(c.get("cost", 0)) < 0:
            errs.append("%s：费用不能为负" % name)
    return errs


def selftest():
    blocks, by_code = load_blocks()
    print("方块表：%d 条" % len(blocks))
    cards, created = load_cards(blocks)
    print("cards.json：%d 张%s" % (len(cards), "（新建）" if created else ""))
    assert all("id" not in c for c in cards), "cards.json 里还有 id 字段"
    assert all("chinese_name" not in c for c in cards), "cards.json 还是 chinese_name，CardInfo 读的是 card_name"
    assert all(isinstance(c["block_kind"], int) for c in cards), "block_kind 必须是方块库下标 int"
    print("字段：%s" % ", ".join(CARD_KEYS))
    errs = validate(cards, blocks)
    if errs:
        print("校验失败：")
        for e in errs:
            print("  - " + e)
        return 1
    print("校验通过")

    probe = []
    added = ensure_all_block_cards(probe, blocks)
    print("全 Block 生成实物卡：%d 张" % added)
    assert ensure_all_block_cards(probe, blocks) == 0, "重复生成没有去重"
    assert len(probe) == len([b for b in blocks if b["code_name"] != "Null"])
    missing = [c for c in probe if not c["card_name"]]
    assert not missing, "生成时没填 card_name：%d 张" % len(missing)
    bad = [c for c in probe if not 0 <= c["block_kind"] < len(blocks)]
    assert not bad, "生成的 block_kind 不是有效下标：%d 张" % len(bad)
    print("生成时下标与中文名已按方块表填好")

    before = json.dumps(cards, ensure_ascii=False, sort_keys=True)
    fd, tmp = tempfile.mkstemp(suffix=".json")
    os.close(fd)
    try:
        doc = {"cards": [{k: c[k] for k in CARD_KEYS} for c in cards]}
        with open(tmp, "w", encoding="utf-8", newline="\n") as f:
            json.dump(doc, f, ensure_ascii=False, indent=2)
        with open(tmp, encoding="utf-8") as f:
            back = json.load(f)["cards"]
        assert before == json.dumps(back, ensure_ascii=False, sort_keys=True), "写盘再回读不一致"
        print("读写往返一致")
    finally:
        os.remove(tmp)
    print("SELFTEST PASS")
    return 0


# ---------------------------------------------------------------- 补全实物卡

def ensure_all_block_cards(cards, blocks):
    """方块库里还没有实物卡的 Block 各补一张，已在的不重复生成。返回新增数量。"""
    have = {int(c.get("block_kind", -1)) for c in cards if int(c.get("card_state", 0)) == 0}
    added = 0
    for i, b in enumerate(blocks):
        if b.get("code_name", "") in ("", "Null") or i in have:
            continue
        cards.append(block_card(b, i))
        have.add(i)
        added += 1
    return added


# ---------------------------------------------------------------- 图标

try:
    from PIL import Image, ImageTk
    HAS_PIL = True
except Exception:                                    # 没装 Pillow 就只显示文字
    HAS_PIL = False

import tkinter as tk
from tkinter import ttk, messagebox, simpledialog

UI_FONT = ("Microsoft YaHei UI", 9)
UI_FONT_S = ("Microsoft YaHei UI", 8)
UI_FONT_B = ("Microsoft YaHei UI", 10, "bold")
UI_FONT_COST = ("Microsoft YaHei UI", 13, "bold")

WALL_BG = "#3c3c3c"
CARD_BG = "#fbfbf7"
CARD_BORDER = "#7a7a7a"
CARD_SELECTED = "#2f8fff"
CARD_W, CARD_H = 150, 198


class IconCache:
    """把 Sprite/<code>.png 缩成指定尺寸的 PhotoImage，按 (code,size) 缓存。"""

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
        self.blocks, self.by_code = load_blocks()
        self.codes = code_names(self.blocks)
        self.code_to_index = {c: i for i, c in enumerate(self.codes)}
        self.cards, created = load_cards(self.blocks)
        self.sel = None                 # 表单正在编辑的那一张
        self.selection = []             # 多选的下标列表
        self.anchor = None
        self.dirty = False
        self.icons = IconCache()
        self.behaviour = []
        self.card_widgets = {}
        self.cols_now = 0

        root.title("卡片预设编辑器 — %s" % CARDS_PATH.name)
        root.geometry("1300x860")
        root.minsize(920, 560)

        style = ttk.Style()
        try:
            style.theme_use("vista")
        except Exception:
            pass
        style.configure("TButton", font=UI_FONT)

        self._build_toolbar()
        self._build_body()
        self.refresh_wall()
        if self.cards:
            self.set_selection([0], anchor=0)
        self.update_status("已生成卡片墙（%d 张）" % len(self.cards) if created else "")

        root.protocol("WM_DELETE_WINDOW", self.on_close)
        root.bind("<Control-s>", lambda e: self.save())
        root.bind("<Escape>", lambda e: self.set_selection([]))
        root.bind("<Control-a>", lambda e: self.set_selection(list(range(len(self.cards)))))
        root.after(120, self._init_sash)

    def _init_sash(self):
        try:
            self.paned.sashpos(0, 860)
        except Exception:
            pass

    # ---------------- 工具栏
    def _build_toolbar(self):
        bar = ttk.Frame(self.root, padding=(8, 6))
        bar.pack(side="top", fill="x")

        def btn(text, cmd, width=None):
            b = ttk.Button(bar, text=text, command=cmd)
            if width:
                b.configure(width=width)
            b.pack(side="left", padx=2)
            return b

        btn("保存", self.save, 7)
        btn("重新载入", self.reload, 9)
        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=8)
        btn("新增卡", self.add_card, 7)
        btn("全部Block生成实物卡", self.gen_all_cards, 18)
        btn("全选", lambda: self.set_selection(list(range(len(self.cards)))), 6)
        btn("清空选择", lambda: self.set_selection([]), 9)
        btn("删除选中", self.delete_selection, 9)
        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=8)
        btn("打开数据目录", self.open_folder, 12)

        ttk.Label(bar, text="筛选").pack(side="left", padx=(14, 2))
        self.filter_var = tk.StringVar()
        ttk.Entry(bar, textvariable=self.filter_var, width=14).pack(side="left")
        self.filter_var.trace_add("write", lambda *a: self.refresh_wall())

    # ---------------- 主体
    def _build_body(self):
        self.status = ttk.Label(self.root, text="", anchor="w", font=UI_FONT)
        self.status.pack(side="bottom", fill="x", padx=10, pady=(0, 6))

        self.paned = ttk.PanedWindow(self.root, orient="horizontal")
        self.paned.pack(side="top", fill="both", expand=True, padx=8, pady=(0, 4))

        left = ttk.Frame(self.paned)
        self.paned.add(left, weight=3)
        self.canvas = tk.Canvas(left, background=WALL_BG, highlightthickness=0)
        sb = ttk.Scrollbar(left, orient="vertical", command=self.canvas.yview)
        self.canvas.configure(yscrollcommand=sb.set)
        self.canvas.pack(side="left", fill="both", expand=True)
        sb.pack(side="right", fill="y")
        self.wall = tk.Frame(self.canvas, background=WALL_BG)
        self.canvas.create_window((0, 0), window=self.wall, anchor="nw", tags="wall")
        self.canvas.bind("<Configure>", self._on_canvas_configure)
        self.canvas.bind("<Button-1>", lambda e: self.set_selection([]))

        right = ttk.Frame(self.paned)
        self.paned.add(right, weight=2)
        self.form_host = ttk.Frame(right)
        self.form_host.pack(fill="both", expand=True)
        self.single_frame = ttk.Frame(self.form_host)
        self.batch_frame = ttk.Frame(self.form_host, padding=(10, 10))
        self._build_form(self.single_frame)
        self._build_batch(self.batch_frame)

    def _on_canvas_configure(self, event):
        self.canvas.itemconfigure("wall", width=event.width)
        cols = max(1, (event.width - 12) // (CARD_W + 12))
        if cols != self.cols_now:
            self.cols_now = cols
            self.refresh_wall()

    def _cols(self):
        return max(1, self.cols_now or 4)

    # ---------------- 卡片墙
    # ---------------- 下标 ↔ 代号
    def code_of(self, index):
        """block_kind 存的是方块库下标，读出来换成代号给界面用。"""
        if 0 <= index < len(self.codes): return self.codes[index]
        return ""

    def index_of(self, code):
        return self.code_to_index.get(code, -1)

    def _match(self, c):
        kw = self.filter_var.get().strip().lower()
        if not kw:
            return True
        code = self.code_of(c.get("block_kind", -1))
        info = self.by_code.get(code, {})
        title = c.get("card_name") or info.get("chinese_name") or code
        return kw in ("%s %s" % (title, code)).lower()

    def card_title(self, c):
        code = self.code_of(c.get("block_kind", -1))
        info = self.by_code.get(code, {})
        return c.get("card_name") or info.get("chinese_name") or (code or "未选方块")

    def refresh_wall(self, keep_scroll=True):
        y = self.canvas.yview()[0] if keep_scroll else 0.0
        for w in self.wall.winfo_children():
            w.destroy()
        self.card_widgets.clear()

        cols = self._cols()
        row = col = 0
        for i, c in enumerate(self.cards):
            if not self._match(c):
                continue
            self._make_card(i, c).grid(row=row, column=col, padx=6, pady=6, sticky="nw")
            col += 1
            if col >= cols:
                col, row = 0, row + 1
        self.wall.update_idletasks()
        self.canvas.configure(scrollregion=self.canvas.bbox("all"))
        self.canvas.yview_moveto(y)
        self._highlight()

    def _make_card(self, i, c):
        code = self.code_of(c.get("block_kind", -1))
        title = self.card_title(c)
        frame = tk.Frame(self.wall, width=CARD_W, height=CARD_H, bg=CARD_BG,
                         highlightthickness=3, highlightbackground=CARD_BORDER)
        frame.grid_propagate(False)
        frame.pack_propagate(False)

        top = tk.Frame(frame, bg=CARD_BG)
        top.pack(fill="x", padx=6, pady=(4, 0))
        cost = tk.Label(top, text=str(c.get("cost", 0)), font=UI_FONT_COST, bg=CARD_BG, fg="#111")
        cost.pack(side="left")
        dele = tk.Label(top, text="✕", font=UI_FONT_S, bg=CARD_BG, fg="#999", cursor="hand2")
        dele.pack(side="right")
        dele.bind("<Button-1>", lambda e, idx=i: self.delete_card(idx))

        name = tk.Label(frame, text=title, font=UI_FONT_B, bg=CARD_BG, fg="#111",
                        wraplength=CARD_W - 18, justify="center")
        name.pack(side="bottom", pady=(0, 8))
        sub = tk.Label(frame, text=self._sub_text(c), font=UI_FONT_S, bg=CARD_BG, fg="#8a8a8a")
        sub.pack(side="bottom", pady=(0, 2))

        img = self.icons.get(code, 84) if int(c.get("card_state", 0)) == 0 else None
        icon = tk.Label(frame, image=img or "", bg=CARD_BG)
        icon.image = img
        icon.pack(side="top", expand=True)

        for w in (frame, top, cost, name, sub, icon):
            w.bind("<Button-1>", lambda e, idx=i: self.on_card_click(idx, e))
            w.bind("<MouseWheel>", self._on_wheel)
        self.card_widgets[i] = {"frame": frame, "cost": cost, "name": name, "sub": sub,
                                "icon": icon, "code": code}
        return frame

    @staticmethod
    def _sub_text(c):
        bits = []
        if int(c.get("card_state", 0)) == 1:
            bits.append("技能牌")
        return "  ".join(bits)

    def _on_wheel(self, event):
        self.canvas.yview_scroll(int(-event.delta / 120), "units")

    def _highlight(self):
        sel = set(self.selection)
        for i, w in self.card_widgets.items():
            w["frame"].configure(highlightbackground=CARD_SELECTED if i in sel else CARD_BORDER)

    def sync_card(self, i=None):
        """把数据同步到墙上的那张卡。"""
        if i is None:
            i = self.sel
        w = self.card_widgets.get(i)
        if w is None or i >= len(self.cards):
            return
        c = self.cards[i]
        code = self.code_of(c.get("block_kind", -1))
        w["cost"].configure(text=str(c.get("cost", 0)))
        w["name"].configure(text=self.card_title(c))
        w["sub"].configure(text=self._sub_text(c))
        img = self.icons.get(code, 84) if int(c.get("card_state", 0)) == 0 else None
        w["icon"].configure(image=img or "")
        w["icon"].image = img
        w["code"] = code

    # ---------------- 选择
    def on_card_click(self, i, event):
        ctrl = bool(event.state & 0x0004)
        shift = bool(event.state & 0x0001)
        if shift and self.anchor is not None:
            lo, hi = sorted((self.anchor, i))
            self.set_selection(list(range(lo, hi + 1)), anchor=self.anchor)
        elif ctrl:
            sel = list(self.selection)
            if i in sel:
                sel.remove(i)
            else:
                sel.append(i)
            self.set_selection(sel, anchor=i)
        else:
            self.set_selection([i], anchor=i)

    def set_selection(self, sel, anchor=None):
        sel = [x for x in dict.fromkeys(sel) if 0 <= x < len(self.cards)]
        self.selection = sel
        if anchor is not None:
            self.anchor = anchor
        elif sel:
            self.anchor = sel[-1]
        else:
            self.anchor = None
        self.after_selection_change()

    def after_selection_change(self):
        if len(self.selection) == 1:
            new = self.selection[0]
            if self.sel != new:
                self.flush_form()
                self.sel = new
                self.load_form(new)
            self.show_panel("single")
        elif self.selection:
            self.flush_form()
            self.sel = self.selection[-1]
            self.show_panel("batch")
        else:
            self.flush_form()
            self.sel = None
            self.show_panel("single")
            self.clear_form()
        self._highlight()
        self.update_status()

    def show_panel(self, kind):
        if kind == "single":
            self.batch_frame.pack_forget()
            if not self.single_frame.winfo_ismapped():
                self.single_frame.pack(fill="both", expand=True)
        else:
            self.single_frame.pack_forget()
            if not self.batch_frame.winfo_ismapped():
                self.batch_frame.pack(fill="both", expand=True)
            self.batch_info.configure(text="已选中 %d 张卡片" % len(self.selection))

    # ---------------- 单张编辑面板
    def _build_form(self, parent):
        self.kind_values = ["(无 / 技能牌)"] + [
            "%s  (%s)" % (b["chinese_name"], b["code_name"])
            for b in self.blocks if b["code_name"] != "Null"]
        self.kind_to_code = {"%s  (%s)" % (b["chinese_name"], b["code_name"]): b["code_name"]
                             for b in self.blocks if b["code_name"] != "Null"}

        box = ttk.Frame(parent, padding=(10, 8))
        box.pack(fill="both", expand=True)
        box.columnconfigure(1, weight=1)

        r = 0

        def row(label, widget):
            nonlocal r
            ttk.Label(box, text=label, font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
            widget.grid(row=r, column=1, sticky="we", pady=3)
            r += 1
            return widget

        self.state_var = tk.StringVar(value=STATE_NAMES[0])
        cb = ttk.Combobox(box, textvariable=self.state_var, values=STATE_NAMES,
                          state="readonly", font=UI_FONT)
        cb.bind("<<ComboboxSelected>>", lambda e: self.apply_live())
        row("卡片状态", cb)

        self.kind_var = tk.StringVar(value=self.kind_values[0])
        cb2 = ttk.Combobox(box, textvariable=self.kind_var, values=self.kind_values,
                           state="readonly", font=UI_FONT)
        cb2.bind("<<ComboboxSelected>>", lambda e: self.on_kind_change())
        row("方块种类", cb2)

        self.cname_var = tk.StringVar()
        e2 = ttk.Entry(box, textvariable=self.cname_var)
        e2.bind("<KeyRelease>", lambda ev: self.apply_live())
        row("中文名", e2)
        ttk.Label(box, text="改名即时反映到卡片上", foreground="#888", font=UI_FONT_S).grid(
            row=r, column=1, sticky="w", pady=(0, 2))
        r += 1

        ttk.Label(box, text="费用", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        numrow = ttk.Frame(box)
        numrow.grid(row=r, column=1, sticky="we", pady=3)
        self.cost_var = tk.StringVar()
        tk.Spinbox(numrow, from_=0, to=99, width=5, textvariable=self.cost_var,
                   command=self.apply_live, font=UI_FONT).pack(side="left")
        r += 1

        ttk.Label(box, text="描述", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        dbox = ttk.Frame(box)
        dbox.grid(row=r, column=1, sticky="nsew", pady=3)
        self.t_desc = tk.Text(dbox, height=4, wrap="word", font=UI_FONT, undo=True)
        self.t_desc.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(dbox, orient="vertical", command=self.t_desc.yview).pack(side="right", fill="y")
        r += 1
        ttk.Label(box, text="留空则用方块库的描述", foreground="#888", font=UI_FONT_S).grid(
            row=r, column=1, sticky="w", pady=(0, 2))
        r += 1

        ttk.Label(box, text="判词", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        abox = ttk.Frame(box)
        abox.grid(row=r, column=1, sticky="nsew", pady=3)
        self.t_adj = tk.Text(abox, height=2, wrap="word", font=UI_FONT, undo=True)
        self.t_adj.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(abox, orient="vertical", command=self.t_adj.yview).pack(side="right", fill="y")
        r += 1

        ttk.Label(box, text="行动词条", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        bbox = ttk.Frame(box)
        bbox.grid(row=r, column=1, sticky="nsew", pady=3)
        self.lb_beh = tk.Listbox(bbox, height=4, font=UI_FONT)
        self.lb_beh.pack(side="left", fill="both", expand=True)
        side = ttk.Frame(bbox)
        side.pack(side="left", fill="y", padx=(6, 0))
        for text, cmd in (("添加", self.beh_add), ("编辑", self.beh_edit), ("删除", self.beh_del)):
            ttk.Button(side, text=text, width=5, command=cmd).pack(pady=1)
        r += 1

        self.t_desc.bind("<KeyRelease>", lambda e: self.apply_live())
        self.t_adj.bind("<KeyRelease>", lambda e: self.apply_live())

    def _build_batch(self, parent):
        ttk.Label(parent, text="多选模式", font=UI_FONT_B).pack(anchor="w")
        self.batch_info = ttk.Label(parent, text="", foreground="#2f8fff", font=UI_FONT)
        self.batch_info.pack(anchor="w", pady=(2, 10))

        self.b_use_cost = tk.BooleanVar(value=True)
        self.b_use_state = tk.BooleanVar(value=False)
        self.b_cost = tk.StringVar(value="1")
        self.b_state = tk.StringVar(value=STATE_NAMES[0])

        def line(var, text, widget):
            f = ttk.Frame(parent)
            f.pack(fill="x", pady=3)
            ttk.Checkbutton(f, text=text, variable=var).pack(side="left")
            widget(f).pack(side="left", padx=(6, 0))

        line(self.b_use_cost, "费用", lambda f: tk.Spinbox(f, from_=0, to=99, width=6,
                                                          textvariable=self.b_cost, font=UI_FONT))
        line(self.b_use_state, "卡片状态", lambda f: ttk.Combobox(f, textvariable=self.b_state,
                                                              values=STATE_NAMES, state="readonly",
                                                              width=8, font=UI_FONT))

        bar = ttk.Frame(parent)
        bar.pack(fill="x", pady=(14, 0))
        ttk.Button(bar, text="应用到选中", command=self.apply_batch).pack(side="left")
        ttk.Button(bar, text="删除选中", command=self.delete_selection).pack(side="left", padx=6)

        ttk.Label(parent, foreground="#888", font=UI_FONT_S, justify="left",
                  text="多选时只能批量改费用和卡片状态。\n"
                       "方块种类、中文名、描述、判词、词条请单选后编辑。").pack(anchor="w", pady=(14, 0))

    # ---------------- 表单读写
    def kind_code(self):
        return self.kind_to_code.get(self.kind_var.get(), "")

    def kind_display(self, code):
        for disp, c in self.kind_to_code.items():
            if c == code:
                return disp
        return self.kind_values[0]

    def on_kind_change(self):
        code = self.kind_code()
        if code and not self.cname_var.get().strip():
            self.cname_var.set(self.by_code.get(code, {}).get("chinese_name", ""))
        self.apply_live()

    @staticmethod
    def _set_text(widget, value):
        widget.delete("1.0", "end")
        widget.insert("1.0", value or "")

    def apply_live(self):
        """输入先落回数据，再刷新那条卡片，避免拿旧值刷新。"""
        self.flush_form()
        self.sync_card()

    def flush_form(self):
        if self.sel is None or self.sel >= len(self.cards):
            return
        if not self.single_frame.winfo_ismapped():
            return
        c = self.cards[self.sel]
        old = json.dumps(c, ensure_ascii=False, sort_keys=True)
        c["card_state"] = 0 if self.state_var.get() == STATE_NAMES[0] else 1
        c["block_kind"] = self.index_of(self.kind_code())
        c["card_name"] = self.cname_var.get().strip()
        c["describe"] = self.t_desc.get("1.0", "end-1c")
        c["adjudication"] = self.t_adj.get("1.0", "end-1c")
        c["cost"] = self.to_int(self.cost_var.get(), 1)
        c["behaviour"] = list(self.behaviour)
        if json.dumps(c, ensure_ascii=False, sort_keys=True) != old:
            self.dirty = True
            self.update_status()

    @staticmethod
    def to_int(text, default):
        try:
            return int(str(text).strip())
        except Exception:
            return default

    def load_form(self, idx):
        c = self.cards[idx]
        st = int(c.get("card_state", 0)) if int(c.get("card_state", 0)) in (0, 1) else 0
        self.state_var.set(STATE_NAMES[st])
        self.kind_var.set(self.kind_display(self.code_of(c.get("block_kind", -1))))
        self.cname_var.set(c.get("card_name", ""))
        self.cost_var.set(str(c.get("cost", 1)))
        self._set_text(self.t_desc, c.get("describe", ""))
        self._set_text(self.t_adj, c.get("adjudication", ""))
        self.behaviour = list(c.get("behaviour") or [])
        self.refresh_behaviour()

    def clear_form(self):
        self.state_var.set(STATE_NAMES[0])
        self.kind_var.set(self.kind_values[0])
        self.cname_var.set("")
        self.cost_var.set("1")
        self._set_text(self.t_desc, "")
        self._set_text(self.t_adj, "")
        self.behaviour = []
        self.refresh_behaviour()

    def refresh_behaviour(self):
        self.lb_beh.delete(0, "end")
        for b in self.behaviour:
            self.lb_beh.insert("end", b)

    def beh_add(self):
        v = simpledialog.askstring("新增词条", "行动词条内容：", parent=self.root)
        if v:
            self.behaviour.append(v)
            self.refresh_behaviour()
            self.flush_form()

    def beh_edit(self):
        sel = self.lb_beh.curselection()
        if not sel:
            return
        v = simpledialog.askstring("编辑词条", "行动词条内容：",
                                   initialvalue=self.behaviour[sel[0]], parent=self.root)
        if v is not None:
            self.behaviour[sel[0]] = v
            self.refresh_behaviour()
            self.flush_form()

    def beh_del(self):
        sel = self.lb_beh.curselection()
        if sel:
            del self.behaviour[sel[0]]
            self.refresh_behaviour()
            self.flush_form()

    # ---------------- 命令
    def add_card(self):
        self.flush_form()
        c = make_card()
        self.cards.append(c)
        self.dirty = True
        self.refresh_wall()
        self.set_selection([len(self.cards) - 1], anchor=len(self.cards) - 1)
        self.update_status("已新增一张卡（费用默认 1）")

    def gen_all_cards(self):
        """方块库里所有还没做成卡的 Block，一次批量生成实物卡。"""
        self.flush_form()
        added = ensure_all_block_cards(self.cards, self.blocks)
        if added:
            self.dirty = True
            self.refresh_wall()
            self.update_status("已生成 %d 张实物卡，当前共 %d 张" % (added, len(self.cards)))
        else:
            self.update_status("所有 Block 都已经是卡片了（%d 张）" % len(self.cards))

    def apply_batch(self):
        if not self.selection:
            return
        cost = self.to_int(self.b_cost.get(), None) if self.b_use_cost.get() else None
        state = (0 if self.b_state.get() == STATE_NAMES[0] else 1) if self.b_use_state.get() else None
        for i in self.selection:
            c = self.cards[i]
            if cost is not None:
                c["cost"] = cost
            if state is not None:
                c["card_state"] = state
            self.sync_card(i)
        self.dirty = True
        self.update_status("已批量修改 %d 张" % len(self.selection))

    def delete_selection(self):
        if not self.selection:
            return
        n = len(self.selection)
        if not messagebox.askyesno("删除", "确定删除选中的 %d 张卡片？" % n):
            return
        self.sel = None
        for i in sorted(self.selection, reverse=True):
            if 0 <= i < len(self.cards):
                del self.cards[i]
        self.selection = []
        self.anchor = None
        self.dirty = True
        self.refresh_wall()
        self.after_selection_change()
        self.update_status("已删除 %d 张" % n)

    def delete_card(self, i):
        c = self.cards[i]
        if not messagebox.askyesno("删除", "确定删除卡片「%s」？" % self.card_title(c)):
            return
        if self.sel == i:
            self.sel = None
        del self.cards[i]
        self.selection = [x for x in self.selection if x != i]
        self.dirty = True
        self.refresh_wall()
        self.after_selection_change()
        self.update_status("已删除 1 张")

    def save(self):
        self.flush_form()
        errs = validate(self.cards, self.blocks)
        if errs:
            messagebox.showwarning("无法保存", "请先修正以下问题：\n\n" + "\n".join(errs[:12]) +
                                   ("\n…" if len(errs) > 12 else ""))
            self.update_status("校验失败 %d 项" % len(errs))
            return False
        try:
            save_cards(self.cards)
        except Exception as exc:
            messagebox.showerror("保存失败", str(exc))
            return False
        self.dirty = False
        self.update_status("已保存 %s（%d 张）" % (CARDS_PATH.name, len(self.cards)))
        return True

    def reload(self):
        if self.dirty and not messagebox.askyesno("重新载入", "有未保存的改动，确定丢弃？"):
            return
        self.blocks, self.by_code = load_blocks()
        self.codes = code_names(self.blocks)
        self.code_to_index = {c: i for i, c in enumerate(self.codes)}
        self.cards, _ = load_cards(self.blocks)
        self.icons = IconCache()
        self.sel = None
        self.selection = []
        self.anchor = None
        self.dirty = False
        self.refresh_wall()
        if self.cards:
            self.set_selection([0], anchor=0)
        else:
            self.after_selection_change()
        self.update_status("已重新载入（%d 张）" % len(self.cards))

    def open_folder(self):
        try:
            os.startfile(str(DATA_DIR))
        except Exception as exc:
            messagebox.showerror("打不开目录", str(exc))

    def update_status(self, extra=""):
        mark = "● 未保存" if self.dirty else "已保存"
        text = "卡片 %d 张 | Block %d 条 | 已选 %d 张 | %s" % (
            len(self.cards), len(self.blocks), len(self.selection), mark)
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
        app.set_selection([0, 1, 2])
        root.update_idletasks()
        print("UI OK：卡片 %d 张，Block %d 条，卡片墙格子 %d 个，列数 %d，多选 %d 张"
              % (len(app.cards), len(app.blocks), len(app.card_widgets),
                 app._cols(), len(app.selection)))
        root.destroy()
        return 0
    root.mainloop()
    return 0


if __name__ == "__main__":
    sys.exit(main())
