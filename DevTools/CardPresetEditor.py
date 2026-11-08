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

PROJECT_ROOT = Path(__file__).resolve().parent.parent
DATA_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Data"
BLOCKLIB_PATH = DATA_DIR / "BlockLibrary.json"
CARDS_PATH = DATA_DIR / "cards.json"
SPRITE_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Sprite"

CARD_KEYS = ["id", "card_state", "block_kind", "chinese_name", "describe",
             "adjudication", "cost", "count", "enabled", "behaviour"]

STATE_NAMES = ["实物牌", "技能牌"]

# 首次生成 cards.json 时的样例：你截图里那 5 张卡，费用 0
SEED = [("laser", "Laser"), ("iron_frame", "Block"), ("drill", "drill"),
        ("shield", "shield"), ("drive_wheel", "PowerWheel")]


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


def make_card(card_state=0, block_kind="", cost=0, count=1, enabled=True):
    return {
        "id": "",
        "card_state": card_state,
        "block_kind": block_kind,
        "chinese_name": "",   # 留空 → 代码回退 BlockLibrary
        "describe": "",       # 留空 → 代码回退 BlockLibrary
        "adjudication": "",   # 留空 → 代码回退 BlockLibrary
        "cost": cost,
        "count": count,
        "enabled": enabled,
        "behaviour": [],
    }


def seed_cards():
    out = []
    for cid, code in SEED:
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

# ---------------------------------------------------------------- 图标

try:
    from PIL import Image, ImageTk
    HAS_PIL = True
except Exception:                                    # 没装 Pillow 就只显示文字
    HAS_PIL = False

import tkinter as tk
from tkinter import ttk, messagebox, filedialog

UI_FONT = ("Microsoft YaHei UI", 9)
UI_FONT_B = ("Microsoft YaHei UI", 9, "bold")
TITLE_FONT = ("Microsoft YaHei UI", 11, "bold")


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

        root.title("卡片预设编辑器 — %s" % CARDS_PATH.name)
        root.geometry("1240x820")
        root.minsize(1000, 640)

        style = ttk.Style()
        try:
            style.theme_use("vista")
        except Exception:
            pass
        style.configure("Treeview", rowheight=44, font=UI_FONT)
        style.configure("TButton", font=UI_FONT)

        self._build_toolbar()
        self._build_body()
        self.refresh_list()
        if self.cards:
            self.refresh_list(keep_index=0)
            self.on_select()
        self.update_status("已生成样例 cards.json" if created else "")
        self.root.after(80, self._init_sash)

        root.protocol("WM_DELETE_WINDOW", self.on_close)
        root.bind("<Control-s>", lambda e: self.save())
        root.bind("<Control-S>", lambda e: self.save())

    def _init_sash(self):
        try:
            self.paned.sashpos(0, 380)
        except Exception:
            pass

    # ---------------- 顶部工具栏
    def _build_toolbar(self):
        bar = ttk.Frame(self.root, padding=(8, 6))
        bar.pack(side="top", fill="x")

        def btn(text, cmd, width=8):
            b = ttk.Button(bar, text=text, command=cmd, width=width)
            b.pack(side="left", padx=2)
            return b

        btn("保存", self.save, 7)
        btn("重新载入", self.reload, 9)
        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=8)
        btn("新增", self.add_card, 6)
        btn("批量导入", self.import_bulk, 9)
        btn("复制", self.duplicate_card, 6)
        btn("删除", self.delete_card, 6)
        btn("上移", lambda: self.move_card(-1), 6)
        btn("下移", lambda: self.move_card(1), 6)
        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=8)
        btn("打开数据目录", self.open_folder, 12)

        ttk.Label(bar, text="筛选").pack(side="left", padx=(14, 2))
        self.filter_var = tk.StringVar()
        ent = ttk.Entry(bar, textvariable=self.filter_var, width=18)
        ent.pack(side="left")
        self.filter_var.trace_add("write", lambda *a: self.refresh_list())

    # ---------------- 主体：左列表 + 右表单
    def _build_body(self):
        # 状态栏先 pack，否则会被上面的面板吃掉高度
        self.status = ttk.Label(self.root, text="", anchor="w", font=UI_FONT)
        self.status.pack(side="bottom", fill="x", padx=10, pady=(0, 6))

        self.paned = ttk.PanedWindow(self.root, orient="horizontal")
        self.paned.pack(side="top", fill="both", expand=True, padx=8, pady=(0, 4))

        # 左：卡片列表
        left = ttk.Frame(self.paned)
        self.paned.add(left, weight=1)
        cols = ("cost", "count", "kind")
        self.tree = ttk.Treeview(left, columns=cols, show="tree headings", selectmode="browse")
        self.tree.heading("#0", text="卡片")
        self.tree.heading("cost", text="费用")
        self.tree.heading("count", text="张数")
        self.tree.heading("kind", text="方块")
        self.tree.column("#0", width=170)
        self.tree.column("cost", width=48, anchor="center")
        self.tree.column("count", width=48, anchor="center")
        self.tree.column("kind", width=115)
        sb = ttk.Scrollbar(left, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscrollcommand=sb.set)
        self.tree.pack(side="left", fill="both", expand=True)
        sb.pack(side="right", fill="y")
        self.tree.bind("<<TreeviewSelect>>", self.on_select)
        # 右：编辑表单
        right = ttk.Frame(self.paned)
        self.paned.add(right, weight=3)
        self._build_form(right)

    # ---------------- 列表刷新
    def refresh_list(self, keep_index=None):
        kw = self.filter_var.get().strip().lower()
        self.tree.delete(*self.tree.get_children())
        for i, c in enumerate(self.cards):
            code = c.get("block_kind", "")
            info = self.by_code.get(code, {})
            title = c.get("chinese_name") or info.get("chinese_name") or (code or "(未选方块)")
            if kw and kw not in (title + " " + str(c.get("id", "")) + " " + code).lower():
                continue
            img = self.icons.get(code, 40)
            text = title if c.get("enabled", True) else "◌ " + title
            self.tree.insert("", "end", iid=str(i), text="  " + text, image=img or "",
                             values=(c.get("cost", 0), c.get("count", 0), code or "-"))
        if keep_index is not None and self.tree.exists(str(keep_index)):
            self.tree.selection_set(str(keep_index))
            self.tree.see(str(keep_index))

    def on_select(self, _evt=None):
        sel = self.tree.selection()
        if not sel:
            return
        idx = int(sel[0])
        if idx == self.sel:
            return
        self.flush_form()
        self.sel = idx
        self.load_form(idx)

    def flush_form(self):
        """把表单里的内容写回当前选中的卡片字典。"""
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
        c["enabled"] = bool(self.enabled_var.get())
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

    # ---------------- 右侧表单
    def _build_form(self, parent):
        self.kind_values = ["(无 / 技能牌)"] + [
            "%s  (%s)" % (b["chinese_name"], b["code_name"])
            for b in self.blocks if b["code_name"] != "Null"
        ]
        self.kind_to_code = {}
        for b in self.blocks:
            if b["code_name"] != "Null":
                self.kind_to_code["%s  (%s)" % (b["chinese_name"], b["code_name"])] = b["code_name"]

        # 预览面板要先 pack，否则会被主表单挤到右下角
        prev = ttk.LabelFrame(parent, text=" 卡片预览 ", padding=12)
        prev.pack(side="right", fill="y", padx=(4, 6), pady=6)
        self.preview_icon = ttk.Label(prev)
        self.preview_icon.pack(pady=(4, 8))
        self.preview_cost = ttk.Label(prev, text="0", font=TITLE_FONT)
        self.preview_cost.pack()
        self.preview_name = ttk.Label(prev, text="—", font=UI_FONT_B, wraplength=160, justify="center")
        self.preview_name.pack(pady=(6, 2))
        self.preview_kind = ttk.Label(prev, text="", foreground="#777", font=UI_FONT,
                                      wraplength=160, justify="center")
        self.preview_kind.pack()

        wrap = ttk.Frame(parent, padding=(10, 6))
        wrap.pack(side="left", fill="both", expand=True)
        wrap.columnconfigure(1, weight=1)

        r = 0
        def row(label, widget, hint=""):
            nonlocal r
            ttk.Label(wrap, text=label, font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=4, padx=(0, 8))
            widget.grid(row=r, column=1, sticky="we", pady=4)
            if hint:
                ttk.Label(wrap, text=hint, foreground="#777", font=UI_FONT).grid(
                    row=r, column=2, sticky="w", padx=(8, 0))
            r += 1
            return widget

        self.id_var = tk.StringVar()
        row("ID", ttk.Entry(wrap, textvariable=self.id_var), "代码引用用的唯一键，如 laser")

        self.state_var = tk.StringVar(value=STATE_NAMES[0])
        cb_state = ttk.Combobox(wrap, textvariable=self.state_var, values=STATE_NAMES,
                                state="readonly", font=UI_FONT)
        cb_state.bind("<<ComboboxSelected>>", lambda e: self.sync_row())
        row("卡片状态", cb_state)

        self.kind_var = tk.StringVar(value=self.kind_values[0])
        cb_kind = ttk.Combobox(wrap, textvariable=self.kind_var, values=self.kind_values,
                               state="readonly", font=UI_FONT)
        cb_kind.bind("<<ComboboxSelected>>", lambda e: self.on_kind_change())
        row("方块种类", cb_kind, "换种类会同步换图标")

        self.cname_var = tk.StringVar()
        e = ttk.Entry(wrap, textvariable=self.cname_var)
        e.bind("<KeyRelease>", lambda ev: self.sync_row())
        row("中文名", e, "留空 → 用方块库中文名")

        numrow = ttk.Frame(wrap)
        numrow.grid(row=r, column=1, sticky="we", pady=4)
        ttk.Label(wrap, text="费用 / 张数", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=4, padx=(0, 8))
        self.cost_var = tk.StringVar()
        self.count_var = tk.StringVar()
        sp1 = tk.Spinbox(numrow, from_=0, to=99, width=6, textvariable=self.cost_var,
                         command=self.sync_row, font=UI_FONT)
        sp1.pack(side="left")
        ttk.Label(numrow, text="张").pack(side="left", padx=(10, 4))
        sp2 = tk.Spinbox(numrow, from_=0, to=99, width=6, textvariable=self.count_var,
                         command=self.sync_row, font=UI_FONT)
        sp2.pack(side="left")
        r += 1

        self.enabled_var = tk.BooleanVar(value=True)
        ttk.Checkbutton(wrap, text="启用（关掉后不进牌堆）", variable=self.enabled_var,
                        command=self.sync_row, style="TCheckbutton").grid(
            row=r, column=1, sticky="w", pady=4)
        r += 1

        ttk.Label(wrap, text="描述", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=4, padx=(0, 8))
        desc_box = ttk.Frame(wrap)
        desc_box.grid(row=r, column=1, sticky="nsew", pady=4)
        self.t_desc = tk.Text(desc_box, height=5, wrap="word", font=UI_FONT, undo=True)
        self.t_desc.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(desc_box, orient="vertical", command=self.t_desc.yview).pack(side="right", fill="y")
        ttk.Label(wrap, text="留空 → 方块库描述", foreground="#777", font=UI_FONT).grid(
            row=r, column=2, sticky="w", padx=(8, 0))
        r += 1

        ttk.Label(wrap, text="判词", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=4, padx=(0, 8))
        adj_box = ttk.Frame(wrap)
        adj_box.grid(row=r, column=1, sticky="nsew", pady=4)
        self.t_adj = tk.Text(adj_box, height=2, wrap="word", font=UI_FONT, undo=True)
        self.t_adj.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(adj_box, orient="vertical", command=self.t_adj.yview).pack(side="right", fill="y")
        ttk.Label(wrap, text="留空 → 方块库判词", foreground="#777", font=UI_FONT).grid(
            row=r, column=2, sticky="w", padx=(8, 0))
        r += 1

        ttk.Label(wrap, text="行动词条", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=4, padx=(0, 8))
        beh_box = ttk.Frame(wrap)
        beh_box.grid(row=r, column=1, sticky="nsew", pady=4)
        self.lb_beh = tk.Listbox(beh_box, height=5, font=UI_FONT)
        self.lb_beh.pack(side="left", fill="both", expand=True)
        side = ttk.Frame(beh_box)
        side.pack(side="left", fill="y", padx=(6, 0))
        ttk.Button(side, text="添加", width=6, command=self.beh_add).pack(pady=1)
        ttk.Button(side, text="编辑", width=6, command=self.beh_edit).pack(pady=1)
        ttk.Button(side, text="删除", width=6, command=self.beh_del).pack(pady=1)
        ttk.Button(side, text="上移", width=6, command=lambda: self.beh_move(-1)).pack(pady=1)
        ttk.Button(side, text="下移", width=6, command=lambda: self.beh_move(1)).pack(pady=1)
        r += 1

    def kind_code(self):
        v = self.kind_var.get()
        if v in self.kind_to_code:
            return self.kind_to_code[v]
        return ""

    def kind_display(self, code):
        for disp, c in self.kind_to_code.items():
            if c == code:
                return disp
        return self.kind_values[0]

    def on_kind_change(self):
        self.sync_row()
        self.update_preview()

    def sync_row(self):
        """表单改动后立刻刷新左列表对应行与预览。"""
        self.flush_form()
        if self.sel is None:
            return
        c = self.cards[self.sel]
        code = c.get("block_kind", "")
        info = self.by_code.get(code, {})
        title = c.get("chinese_name") or info.get("chinese_name") or (code or "(未选方块)")
        img = self.icons.get(code, 40)
        if self.tree.exists(str(self.sel)):
            self.tree.item(str(self.sel), text="  " + (title if c.get("enabled", True) else "◌ " + title),
                           image=img or "", values=(c.get("cost", 0), c.get("count", 0), code or "-"))
        self.update_preview()

    def update_preview(self):
        code = self.kind_code() if self.state_var.get() == STATE_NAMES[0] else ""
        info = self.by_code.get(code, {})
        self.preview_icon.configure(image=self.icons.get(code, 96) or "")
        self.preview_icon.image = self.icons.get(code, 96)
        self.preview_cost.configure(text=str(self.to_int(self.cost_var.get(), 0)))
        name = self.cname_var.get() or info.get("chinese_name") or "(未命名)"
        self.preview_name.configure(text=name)
        self.preview_kind.configure(text=(code or "技能牌") if self.state_var.get() == STATE_NAMES[0] else "技能牌")

    def load_form(self, idx):
        c = self.cards[idx]
        self.id_var.set(c.get("id", ""))
        self.state_var.set(STATE_NAMES[int(c.get("card_state", 0)) if int(c.get("card_state", 0)) in (0, 1) else 0])
        self.kind_var.set(self.kind_display(c.get("block_kind", "")))
        self.cname_var.set(c.get("chinese_name", ""))
        self.cost_var.set(str(c.get("cost", 0)))
        self.count_var.set(str(c.get("count", 1)))
        self.enabled_var.set(bool(c.get("enabled", True)))
        for widget, key in ((self.t_desc, "describe"), (self.t_adj, "adjudication")):
            widget.delete("1.0", "end")
            widget.insert("1.0", c.get(key, "") or "")
        self.behaviour = list(c.get("behaviour") or [])
        self.refresh_behaviour()
        self.update_preview()
        self.set_form_enabled(True)

    def set_form_enabled(self, flag):
        state = "normal" if flag else "disabled"
        for w in (self.t_desc, self.t_adj, self.lb_beh):
            try:
                w.configure(state=state)
            except Exception:
                pass

    def refresh_behaviour(self):
        self.lb_beh.delete(0, "end")
        for b in self.behaviour:
            self.lb_beh.insert("end", b)

    def beh_add(self):
        from tkinter import simpledialog
        v = simpledialog.askstring("新增词条", "行动词条内容：", parent=self.root)
        if v:
            self.behaviour.append(v)
            self.refresh_behaviour()
            self.sync_row()

    def beh_edit(self):
        from tkinter import simpledialog
        sel = self.lb_beh.curselection()
        if not sel:
            return
        v = simpledialog.askstring("编辑词条", "行动词条内容：",
                                   initialvalue=self.behaviour[sel[0]], parent=self.root)
        if v is not None:
            self.behaviour[sel[0]] = v
            self.refresh_behaviour()
            self.sync_row()

    def beh_del(self):
        sel = self.lb_beh.curselection()
        if sel:
            del self.behaviour[sel[0]]
            self.refresh_behaviour()
            self.sync_row()

    def beh_move(self, delta):
        sel = self.lb_beh.curselection()
        if not sel:
            return
        i = sel[0]
        j = i + delta
        if 0 <= j < len(self.behaviour):
            self.behaviour[i], self.behaviour[j] = self.behaviour[j], self.behaviour[i]
            self.refresh_behaviour()
            self.lb_beh.selection_set(j)
            self.sync_row()

    # ---------------- 编辑命令
    def unique_id(self, base):
        ids = {c.get("id") for c in self.cards}
        if base not in ids:
            return base
        i = 2
        while "%s_%d" % (base, i) in ids:
            i += 1
        return "%s_%d" % (base, i)

    def add_card(self):
        self.flush_form()
        c = make_card()
        c["id"] = self.unique_id("card")
        pos = len(self.cards) if self.sel is None else self.sel + 1
        self.cards.insert(pos, c)
        self.dirty = True
        self.refresh_list(keep_index=pos)
        self.sel = None
        self.on_select()
        self.update_status("已新增一张卡")

    def duplicate_card(self):
        self.flush_form()
        if self.sel is None:
            return
        c = dict(self.cards[self.sel])
        c["behaviour"] = list(c.get("behaviour") or [])
        c["id"] = self.unique_id((c.get("id") or "card") + "_copy")
        pos = self.sel + 1
        self.cards.insert(pos, c)
        self.dirty = True
        self.refresh_list(keep_index=pos)
        self.sel = None
        self.on_select()

    def delete_card(self):
        self.flush_form()
        if self.sel is None:
            return
        c = self.cards[self.sel]
        if not messagebox.askyesno("删除", "确定删除卡片 %s ？" % (c.get("id") or "第 %d 张" % (self.sel + 1))):
            return
        del self.cards[self.sel]
        self.dirty = True
        self.sel = None
        self.refresh_list()
        if self.cards:
            self.refresh_list(keep_index=min(self.sel or 0, len(self.cards) - 1))
            self.on_select()
        else:
            self.clear_form()
        self.update_status("已删除")

    def move_card(self, delta):
        self.flush_form()
        if self.sel is None:
            return
        j = self.sel + delta
        if not (0 <= j < len(self.cards)):
            return
        self.cards[self.sel], self.cards[j] = self.cards[j], self.cards[self.sel]
        self.dirty = True
        self.refresh_list(keep_index=j)
        self.sel = None
        self.on_select()

    def clear_form(self):
        self.sel = None
        self.id_var.set("")
        self.cname_var.set("")
        self.cost_var.set("0")
        self.count_var.set("1")
        self.enabled_var.set(True)
        for w in (self.t_desc, self.t_adj):
            w.delete("1.0", "end")
        self.behaviour = []
        self.refresh_behaviour()
        self.update_preview()

    def import_bulk(self):
        top = tk.Toplevel(self.root)
        top.title("从方块库批量导入")
        top.geometry("420x560")
        ttk.Label(top, text="选择要变成卡片的方块（可 Ctrl/Shift 多选）：",
                  font=UI_FONT).pack(anchor="w", padx=10, pady=(10, 4))
        frame = ttk.Frame(top)
        frame.pack(fill="both", expand=True, padx=10)
        lb = tk.Listbox(frame, selectmode="extended", font=UI_FONT)
        lb.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(frame, orient="vertical", command=lb.yview).pack(side="right", fill="y")
        items = [b for b in self.blocks if b["code_name"] != "Null"]
        for b in items:
            lb.insert("end", "%s  (%s)" % (b["chinese_name"], b["code_name"]))

        def do_import():
            picked = [items[i] for i in lb.curselection()]
            if not picked:
                return
            self.flush_form()
            for b in picked:
                code = b["code_name"]
                c = make_card(block_kind=code)
                base = "".join(ch if ch.isalnum() else "_" for ch in code.lower())
                c["id"] = self.unique_id(base)
                self.cards.append(c)
            self.dirty = True
            self.refresh_list()
            top.destroy()
            self.update_status("已导入 %d 项" % len(picked))

        bar = ttk.Frame(top)
        bar.pack(fill="x", padx=10, pady=10)
        ttk.Button(bar, text="全选", command=lambda: lb.selection_set(0, "end")).pack(side="left")
        ttk.Button(bar, text="导入", command=do_import).pack(side="right")
        ttk.Button(bar, text="取消", command=top.destroy).pack(side="right", padx=6)

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
        self.refresh_list(keep_index=self.sel)
        self.update_status("已保存 %s" % CARDS_PATH.name)
        return True

    def reload(self):
        if self.dirty and not messagebox.askyesno("重新载入", "有未保存的改动，确定丢弃？"):
            return
        self.blocks, self.by_code = load_blocks()
        self.cards, created = load_cards()
        self.icons = IconCache()
        self.sel = None
        self.dirty = False
        self.refresh_list()
        self.clear_form()
        if self.cards:
            self.refresh_list(keep_index=0)
            self.on_select()
        self.update_status("已重新载入")

    def open_folder(self):
        try:
            os.startfile(str(DATA_DIR))
        except Exception as exc:
            messagebox.showerror("打不开目录", str(exc))

    def update_status(self, extra=""):
        mark = "● 未保存" if self.dirty else "已保存"
        text = "卡片 %d 张 | 方块表 %d 条 | %s" % (len(self.cards), len(self.blocks), mark)
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
        for i in range(3):
            if i < len(app.cards):
                root.after(10)
        print("UI OK：卡片 %d 张，方块 %d 条，列表行 %d"
              % (len(app.cards), len(app.blocks), len(app.tree.get_children())))
        root.destroy()
        return 0
    root.mainloop()
    return 0


if __name__ == "__main__":
    sys.exit(main())
