# -*- coding: utf-8 -*-
"""卡片预设编辑器（仅开发期使用）

- 数据源：Assets/Resources/Data/BlockLibrary.json（方块表，只读）
- 编辑对象：Assets/Resources/Data/cards.json（卡片预设，可读可写）
- 图标：Assets/Resources/Sprite/<code_name>.png（大小写不敏感）

用法：
    python DevTools/CardPresetEditor.py            # 打开窗口
    python DevTools/CardPresetEditor.py --selftest # 只跑数据读写自检
    python DevTools/CardPresetEditor.py --smoke    # 建窗口后自动关闭，验证界面能起来
"""

import json
import os
import sys
import tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent


def find_project_root():
    """从脚本位置往上找，认 ProjectSettings + Assets 同时存在的目录为工程根。
    这样工具放在 Assets 里还是工程根目录下都能跑。"""
    for p in [HERE] + list(HERE.parents):
        if (p / "Assets").is_dir() and (p / "ProjectSettings").is_dir():
            return p
    return HERE.parent


PROJECT_ROOT = find_project_root()
DATA_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Data"
BLOCKLIB_PATH = DATA_DIR / "BlockLibrary.json"
CARDS_PATH = DATA_DIR / "cards.json"
SPRITE_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Sprite"

CARD_KEYS = ["id", "card_state", "block_kind", "chinese_name", "describe",
             "adjudication", "cost", "count", "behaviour"]

STATE_NAMES = ["实物牌", "技能牌"]

# 方块库读不到时的兜底样例
FALLBACK_SEED = [("laser", "Laser"), ("block", "Block"), ("drill", "drill"),
                 ("shield", "shield"), ("power_wheel", "PowerWheel")]


# ---------------------------------------------------------------- 数据层

def load_blocks():
    """读方块表，返回 (list, {code_name: info})。"""
    with open(BLOCKLIB_PATH, encoding="utf-8") as f:
        doc = json.load(f)
    blocks = doc.get("blocks") or []
    by_code = {b["code_name"]: b for b in blocks}
    return blocks, by_code


def sprite_index():
    """code_name(小写) -> png 路径，用来兼容 Engine/engine.png 这种大小写不一致。"""
    idx = {}
    if SPRITE_DIR.is_dir():
        for p in SPRITE_DIR.iterdir():
            if p.suffix.lower() == ".png":
                idx.setdefault(p.stem.lower(), p)
    return idx


def make_card(card_state=0, block_kind="", cost=0, count=1):
    return {
        "id": "",
        "card_state": card_state,
        "block_kind": block_kind,
        "chinese_name": "",   # 留空 → 代码回退 BlockLibrary
        "describe": "",       # 留空 → 代码回退 BlockLibrary
        "adjudication": "",   # 留空 → 代码回退 BlockLibrary
        "cost": cost,
        "count": count,
        "behaviour": [],
    }


def to_id(code):
    """code_name -> 代码引用用的 id：CamelCase / 连字符统一成小写下划线。"""
    s = "".join(ch if (ch.isalnum() and ch.isascii()) else "_" for ch in code)
    out = []
    for i, ch in enumerate(s):
        if ch.isupper() and i > 0 and not s[i - 1].isupper():
            out.append("_")
        out.append(ch.lower())
    return "_".join(p for p in "".join(out).split("_") if p)


def seed_cards():
    """默认把方块库里每个方块都建成一张卡（跳过占位的 Null）。"""
    try:
        blocks, _ = load_blocks()
    except Exception:
        blocks = []
    out = []
    used = set()
    for b in blocks:
        code = b.get("code_name", "")
        if not code or code == "Null":
            continue
        c = make_card(block_kind=code)
        cid = to_id(code) or "card"
        base, n = cid, 1
        while cid in used:
            n += 1
            cid = "%s_%d" % (base, n)
        used.add(cid)
        c["id"] = cid
        out.append(c)
    if out:
        return out
    for cid, code in FALLBACK_SEED:
        c = make_card(block_kind=code)
        c["id"] = cid
        out.append(c)
    return out


def load_cards():
    """cards.json 不存在就生成样例；返回 (cards, created)。"""
    if not CARDS_PATH.exists():
        cards = seed_cards()
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
        c["behaviour"] = [str(x) for x in (c.get("behaviour") or [])]
        cards.append(c)
    return cards, False


def save_cards(cards):
    """按固定字段顺序、UTF-8 无 BOM、LF 写盘；先写临时文件再替换，避免写坏。"""
    doc = {"version": 1, "cards": [{k: c[k] for k in CARD_KEYS} for c in cards]}
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


def validate(cards, by_code):
    """返回错误描述列表，空列表代表可以保存。"""
    errs = []
    seen = {}
    for i, c in enumerate(cards):
        cid = str(c.get("id", "")).strip()
        name = cid or ("第 %d 张" % (i + 1))
        if not cid:
            errs.append("%s：id 不能为空" % name)
        elif cid in seen:
            errs.append("%s：id 与第 %d 张重复" % (cid, seen[cid] + 1))
        else:
            seen[cid] = i
        if int(c.get("card_state", 0)) not in (0, 1):
            errs.append("%s：卡片状态只能是 0 或 1" % name)
        if int(c.get("card_state", 0)) == 0:
            code = str(c.get("block_kind", ""))
            if not code:
                errs.append("%s：实物牌必须选方块种类" % name)
            elif code not in by_code:
                errs.append("%s：方块种类 %s 不在 BlockLibrary 里" % (name, code))
        if int(c.get("cost", 0)) < 0:
            errs.append("%s：费用不能为负" % name)
        if int(c.get("count", 0)) < 0:
            errs.append("%s：牌堆张数不能为负" % name)
    return errs


def selftest():
    blocks, by_code = load_blocks()
    print("方块表：%d 条" % len(blocks))
    cards, created = load_cards()
    print("cards.json：%d 张%s" % (len(cards), "（新建）" if created else ""))
    errs = validate(cards, by_code)
    if errs:
        print("校验失败：")
        for e in errs:
            print("  - " + e)
        return 1
    probe = []
    added = ensure_all_block_cards(probe, blocks)
    print("全 Block 生成实物卡：%d 张（应为方块数-1）" % added)
    again = ensure_all_block_cards(probe, blocks)
    assert again == 0, "重复生成没有去重"
    assert len(probe) == len([b for b in blocks if b["code_name"] != "Null"])
    print("校验通过")
    before = json.dumps(cards, ensure_ascii=False, sort_keys=True)
    fd, tmp = tempfile.mkstemp(suffix=".json")
    os.close(fd)
    try:
        doc = {"version": 1, "cards": [{k: c[k] for k in CARD_KEYS} for c in cards]}
        with open(tmp, "w", encoding="utf-8", newline="\n") as f:
            json.dump(doc, f, ensure_ascii=False, indent=2)
        with open(tmp, encoding="utf-8") as f:
            back = json.load(f)["cards"]
        after = json.dumps(back, ensure_ascii=False, sort_keys=True)
        assert before == after, "写盘再回读内容不一致"
        print("读写往返一致")
    finally:
        os.remove(tmp)
    print("SELFTEST PASS")
    return 0


# ---------------------------------------------------------------- 补全实物卡

def ensure_all_block_cards(cards, blocks):
    """把方块库里还没有实物卡的 Block 各补一张，已在的不重复生成。返回新增数量。"""
    have = {c.get("block_kind") for c in cards if int(c.get("card_state", 0)) == 0}
    ids = {c.get("id") for c in cards}
    added = 0
    for b in blocks:
        code = b.get("code_name", "")
        if not code or code == "Null" or code in have:
            continue
        c = make_card(block_kind=code)
        base = to_id(code) or "card"
        cid, n = base, 1
        while cid in ids:
            n += 1
            cid = "%s_%d" % (base, n)
        ids.add(cid)
        c["id"] = cid
        cards.append(c)
        have.add(code)
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
        self.cards, created = load_cards()
        self.sel = None
        self.dirty = False
        self.icons = IconCache()
        self.behaviour = []
        self.card_widgets = {}
        self.cols_now = 0

        root.title("卡片预设编辑器 — %s" % CARDS_PATH.name)
        root.geometry("1280x840")
        root.minsize(900, 560)

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
            self.select(0)
        self.update_status("已生成卡片墙（%d 张）" % len(self.cards) if created else "")

        root.protocol("WM_DELETE_WINDOW", self.on_close)
        root.bind("<Control-s>", lambda e: self.save())
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
        btn("删除选中", self.delete_card, 9)
        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=8)
        btn("打开数据目录", self.open_folder, 12)

        ttk.Label(bar, text="筛选").pack(side="left", padx=(14, 2))
        self.filter_var = tk.StringVar()
        ttk.Entry(bar, textvariable=self.filter_var, width=16).pack(side="left")
        self.filter_var.trace_add("write", lambda *a: self.refresh_wall())

    # ---------------- 主体
    def _build_body(self):
        self.status = ttk.Label(self.root, text="", anchor="w", font=UI_FONT)
        self.status.pack(side="bottom", fill="x", padx=10, pady=(0, 6))

        self.paned = ttk.PanedWindow(self.root, orient="horizontal")
        self.paned.pack(side="top", fill="both", expand=True, padx=8, pady=(0, 4))

        # 左：卡片墙
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

        # 右：编辑面板
        right = ttk.Frame(self.paned)
        self.paned.add(right, weight=2)
        self._build_form(right)

    def _on_canvas_configure(self, event):
        self.canvas.itemconfigure("wall", width=event.width)
        cols = max(1, (event.width - 12) // (CARD_W + 12))
        if cols != self.cols_now:
            self.cols_now = cols
            self.refresh_wall()

    def _cols(self):
        return max(1, self.cols_now or 4)

    # ---------------- 卡片墙
    def _match(self, c):
        kw = self.filter_var.get().strip().lower()
        if not kw:
            return True
        code = c.get("block_kind", "")
        info = self.by_code.get(code, {})
        title = c.get("chinese_name") or info.get("chinese_name") or code
        text = "%s %s %s" % (title, c.get("id", ""), code)
        return kw in text.lower()

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
        code = c.get("block_kind", "")
        info = self.by_code.get(code, {})
        title = c.get("chinese_name") or info.get("chinese_name") or (code or "未选方块")
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
            w.bind("<Button-1>", lambda e, idx=i: self.select(idx))
            w.bind("<MouseWheel>", self._on_wheel)
        self.card_widgets[i] = {"frame": frame, "cost": cost, "name": name, "sub": sub,
                                "icon": icon, "code": code}
        return frame

    @staticmethod
    def _sub_text(c):
        bits = []
        if int(c.get("card_state", 0)) == 1:
            bits.append("技能牌")
        if int(c.get("count", 1)) != 1:
            bits.append("×%d" % int(c.get("count", 1)))
        return "  ".join(bits)

    def _on_wheel(self, event):
        self.canvas.yview_scroll(int(-event.delta / 120), "units")

    def _highlight(self):
        for i, w in self.card_widgets.items():
            w["frame"].configure(highlightbackground=CARD_SELECTED if i == self.sel else CARD_BORDER)

    def select(self, i):
        if i == self.sel:
            self._highlight()
            return
        self.flush_form()
        self.sel = i
        self.load_form(i)
        self._highlight()

    def sync_card(self, i=None):
        """把当前卡片的数据同步到墙上的那张卡（改名/改费用/改种类即时可见）。"""
        if i is None:
            i = self.sel
        w = self.card_widgets.get(i)
        if w is None or i >= len(self.cards):
            return
        c = self.cards[i]
        code = c.get("block_kind", "")
        info = self.by_code.get(code, {})
        w["cost"].configure(text=str(c.get("cost", 0)))
        w["name"].configure(text=c.get("chinese_name") or info.get("chinese_name") or (code or "未选方块"))
        w["sub"].configure(text=self._sub_text(c))
        img = self.icons.get(code, 84) if int(c.get("card_state", 0)) == 0 else None
        w["icon"].configure(image=img or "")
        w["icon"].image = img
        w["code"] = code

    # ---------------- 编辑面板
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

        self.id_var = tk.StringVar()
        e = ttk.Entry(box, textvariable=self.id_var)
        e.bind("<KeyRelease>", lambda ev: self.flush_form())
        row("ID", e)

        self.state_var = tk.StringVar(value=STATE_NAMES[0])
        cb = ttk.Combobox(box, textvariable=self.state_var, values=STATE_NAMES,
                          state="readonly", font=UI_FONT)
        cb.bind("<<ComboboxSelected>>", lambda e: self.sync_card())
        row("卡片状态", cb)

        self.kind_var = tk.StringVar(value=self.kind_values[0])
        cb2 = ttk.Combobox(box, textvariable=self.kind_var, values=self.kind_values,
                           state="readonly", font=UI_FONT)
        cb2.bind("<<ComboboxSelected>>", lambda e: self.sync_card())
        row("方块种类", cb2)

        self.cname_var = tk.StringVar()
        e2 = ttk.Entry(box, textvariable=self.cname_var)
        e2.bind("<KeyRelease>", lambda ev: self.sync_card())
        row("中文名", e2)
        ttk.Label(box, text="留空则用方块库的中文名", foreground="#888", font=UI_FONT_S).grid(
            row=r, column=1, sticky="w", pady=(0, 2))
        r += 1

        numrow = ttk.Frame(box)
        ttk.Label(box, text="费用 / 张数", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        numrow.grid(row=r, column=1, sticky="we", pady=3)
        self.cost_var = tk.StringVar()
        self.count_var = tk.StringVar()
        tk.Spinbox(numrow, from_=0, to=99, width=5, textvariable=self.cost_var,
                   command=self.sync_card, font=UI_FONT).pack(side="left")
        ttk.Label(numrow, text="张").pack(side="left", padx=(8, 4))
        tk.Spinbox(numrow, from_=0, to=99, width=5, textvariable=self.count_var,
                   command=self.sync_card, font=UI_FONT).pack(side="left")
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

        self.t_desc.bind("<KeyRelease>", lambda e: self.flush_form())
        self.t_adj.bind("<KeyRelease>", lambda e: self.flush_form())

    def kind_code(self):
        return self.kind_to_code.get(self.kind_var.get(), "")

    def kind_display(self, code):
        for disp, c in self.kind_to_code.items():
            if c == code:
                return disp
        return self.kind_values[0]

    def _set_text(self, widget, value):
        widget.delete("1.0", "end")
        widget.insert("1.0", value or "")

    def flush_form(self):
        if self.sel is None or self.sel >= len(self.cards):
            return
        c = self.cards[self.sel]
        old = json.dumps(c, ensure_ascii=False, sort_keys=True)
        c["id"] = self.id_var.get().strip()
        c["card_state"] = 0 if self.state_var.get() == STATE_NAMES[0] else 1
        code = self.kind_code()
        if code is not None:
            c["block_kind"] = code
        c["chinese_name"] = self.cname_var.get()
        c["describe"] = self.t_desc.get("1.0", "end-1c")
        c["adjudication"] = self.t_adj.get("1.0", "end-1c")
        c["cost"] = self.to_int(self.cost_var.get(), 0)
        c["count"] = self.to_int(self.count_var.get(), 1)
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
        self.id_var.set(c.get("id", ""))
        st = int(c.get("card_state", 0)) if int(c.get("card_state", 0)) in (0, 1) else 0
        self.state_var.set(STATE_NAMES[st])
        self.kind_var.set(self.kind_display(c.get("block_kind", "")))
        self.cname_var.set(c.get("chinese_name", ""))
        self.cost_var.set(str(c.get("cost", 0)))
        self.count_var.set(str(c.get("count", 1)))
        self._set_text(self.t_desc, c.get("describe", ""))
        self._set_text(self.t_adj, c.get("adjudication", ""))
        self.behaviour = list(c.get("behaviour") or [])
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
        c["id"] = self.unique_id("card")
        self.cards.append(c)
        self.dirty = True
        self.refresh_wall()
        self.select(len(self.cards) - 1)
        self.update_status("已新增一张卡")

    def unique_id(self, base):
        ids = {c.get("id") for c in self.cards}
        if base not in ids:
            return base
        i = 2
        while "%s_%d" % (base, i) in ids:
            i += 1
        return "%s_%d" % (base, i)

    def gen_all_cards(self):
        """点一下：方块库里所有还没做成卡的 Block，批量生成实物卡。"""
        self.flush_form()
        added = ensure_all_block_cards(self.cards, self.blocks)
        if added:
            self.dirty = True
            self.refresh_wall()
            self.update_status("已生成 %d 张实物卡，当前共 %d 张" % (added, len(self.cards)))
        else:
            self.update_status("所有 Block 都已经是卡片了（%d 张）" % len(self.cards))

    def delete_card(self, i=None):
        if i is None:
            i = self.sel
        if i is None or i >= len(self.cards):
            return
        c = self.cards[i]
        if not messagebox.askyesno("删除", "确定删除卡片 %s ？" % (c.get("id") or "第 %d 张" % (i + 1))):
            return
        self.sel = None
        del self.cards[i]
        self.dirty = True
        self.refresh_wall()
        if self.cards:
            self.select(min(i, len(self.cards) - 1))
        self.update_status("已删除一张卡")

    def save(self):
        self.flush_form()
        errs = validate(self.cards, self.by_code)
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
        self.cards, _ = load_cards()
        self.icons = IconCache()
        self.sel = None
        self.dirty = False
        self.refresh_wall()
        if self.cards:
            self.select(0)
        self.update_status("已重新载入（%d 张）" % len(self.cards))

    def open_folder(self):
        try:
            os.startfile(str(DATA_DIR))
        except Exception as exc:
            messagebox.showerror("打不开目录", str(exc))

    def update_status(self, extra=""):
        mark = "● 未保存" if self.dirty else "已保存"
        text = "卡片 %d 张 | Block %d 条 | %s" % (len(self.cards), len(self.blocks), mark)
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
        print("UI OK：卡片 %d 张，Block %d 条，卡片墙格子 %d 个，列数 %d"
              % (len(app.cards), len(app.blocks), len(app.card_widgets), app._cols()))
        root.destroy()
        return 0
    root.mainloop()
    return 0


if __name__ == "__main__":
    sys.exit(main())
