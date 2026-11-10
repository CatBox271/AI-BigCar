# -*- coding: utf-8 -*-
"""位置效果编辑器（仅开发期使用）

- 编辑对象：Assets/Resources/Data/LocationEffect.json（可读可写）
- 图标：Assets/Resources/Sprite/<pic_name>.png（大小写不敏感）
- 字段与 LocationEffect 一一对应：pic_name / rarity / circle / describe / needs / effects
- rarity 是 float，抽中概率 = (1/rarity) / Σ(1/rarity)，卡片上直接显示算出来的百分比
- circle 是位置正态的 µ；圈层是长方形，中间三个是圈 0，每圈往四周各扩 1 格

用法：
    python Assets/DevTools/LocationEffectEditor.py            # 打开窗口
    python Assets/DevTools/LocationEffectEditor.py --selftest # 只跑数据自检
    python Assets/DevTools/LocationEffectEditor.py --smoke    # 建窗口后自动关闭
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
LOCATION_EFFECT_PATH = DATA_DIR / "LocationEffect.json"
SPRITE_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Sprite"

# 与 LocationEffect 的字段一一对应（JsonUtility 只认同名字段，枚举存 int）
EFFECT_KEYS = ["pic_name", "rarity", "circle", "describe", "needs", "effects"]
NEED_KEYS = ["need", "needpara"]
CAR_EFFECT_KEYS = ["effectKind", "effectNum", "forever"]

# 与 LocationEffect.Needed 一一对应
NEEDED_VALUES = [
    "方块代号 (block_code_name)",
    "被完全填满 (fullfill)",
    "载具第 N 格 + 此格 (both)",
    "此格没有任何物体 (nonefill)",
    "两者都没有 (nor)",
]

# 与 LocationEffect.EffectKind 一一对应
EFFECTKIND_VALUES = [
    "载具数 + (vn_a)",
    "载具数 × (vn_m)",
    "生命 % + (hp_p)",
    "速度 % + (vel_p)",
    "命中 % + (hit_p)",
    "体型 % + (size_p)",
    "冒险效率 % + (adv_p)",
]

# 圈层预览的默认网格，和卡牌模式场景里的 ColliderArea 一致
GRID_W, GRID_H = 7, 5


def to_float(text, default):
    try:
        return float(str(text).strip())
    except Exception:
        return default


def to_int(text, default):
    try:
        return int(str(text).strip())
    except Exception:
        return default


def make_effect():
    return {"pic_name": "", "rarity": 1.0, "circle": 0.0,
            "describe": "", "needs": [], "effects": []}


def make_need(need=0, needpara=0.0):
    return {"need": int(need), "needpara": float(needpara)}


def make_car_effect(effect_kind=0, effect_num=0.0, forever=False):
    return {"effectKind": int(effect_kind), "effectNum": float(effect_num), "forever": bool(forever)}


def need_text(n):
    k = n["need"] if 0 <= n["need"] < len(NEEDED_VALUES) else -1
    label = NEEDED_VALUES[k] if k >= 0 else "未知(%d)" % n["need"]
    return "%s   参数 %g" % (label, n["needpara"])


def car_effect_text(c):
    k = c["effectKind"] if 0 <= c["effectKind"] < len(EFFECTKIND_VALUES) else -1
    label = EFFECTKIND_VALUES[k] if k >= 0 else "未知(%d)" % c["effectKind"]
    return "%s   %g%s" % (label, c["effectNum"], "   永久" if c.get("forever") else "")


def load_effects():
    """LocationEffect.json 不存在就建一份空表；返回 (effects, created)。"""
    if not LOCATION_EFFECT_PATH.exists():
        save_effects([])
        return [], True
    with open(LOCATION_EFFECT_PATH, encoding="utf-8") as f:
        doc = json.load(f)
    effects = []
    for raw in doc.get("effects") or []:
        e = make_effect()
        e["pic_name"] = str(raw.get("pic_name", ""))
        e["describe"] = str(raw.get("describe", ""))
        e["rarity"] = to_float(raw.get("rarity", 1), 1.0)
        e["circle"] = to_float(raw.get("circle", 0), 0.0)
        e["needs"] = [make_need(n.get("need", 0), to_float(n.get("needpara", 0), 0.0))
                      for n in (raw.get("needs") or [])]
        e["effects"] = [make_car_effect(c.get("effectKind", 0), to_float(c.get("effectNum", 0), 0.0),
                                        c.get("forever", False))
                        for c in (raw.get("effects") or [])]
        effects.append(e)
    return effects, False


def save_effects(effects):
    """按固定字段顺序、UTF-8 无 BOM、LF 写盘；先写临时文件再替换。"""
    doc = {"effects": [{k: e[k] for k in EFFECT_KEYS} for e in effects]}
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    fd, tmp = tempfile.mkstemp(dir=str(DATA_DIR), suffix=".tmp")
    os.close(fd)
    try:
        with open(tmp, "w", encoding="utf-8", newline="\n") as f:
            json.dump(doc, f, ensure_ascii=False, indent=2)
            f.write("\n")
        os.replace(tmp, LOCATION_EFFECT_PATH)
    finally:
        if os.path.exists(tmp):
            os.remove(tmp)


def probabilities(effects):
    """稀有度权重反比：概率 = (1/rarity) / Σ(1/rarity)。rarity<=0 的那条按 0 处理。"""
    weights = [(1.0 / e["rarity"]) if e["rarity"] > 0 else 0.0 for e in effects]
    total = sum(weights)
    if total <= 0:
        return [0.0] * len(effects)
    return [w / total * 100.0 for w in weights]



# ---------------------------------------------------------------- 圈层

def seed_rect(w, h):
    """圈 0 是中间那三个：横着摆的三格。"""
    cx, cy = w // 2, h // 2
    return max(cx - 1, 0), min(cx + 1, w - 1), cy, cy


def ring_of(x, y, w, h):
    """格子属于第几圈：从中间三格往四周各扩 1 格算一圈。"""
    x0, x1, y0, y1 = seed_rect(w, h)
    return max(0, x0 - x, x - x1, y0 - y, y - y1)


def ring_cells(w, h):
    """{(x, y): 圈号}"""
    return {(x, y): ring_of(x, y, w, h) for y in range(h) for x in range(w)}


def ring_counts(w, h):
    counts = {}
    for k in ring_cells(w, h).values():
        counts[k] = counts.get(k, 0) + 1
    return counts


def ring_target(circle, w, h):
    """把正态抽到的 circle 值切成圈号：取整再夹到 [0, 最大圈]。"""
    top = max(ring_counts(w, h))
    return int(max(0, min(top, round(circle))))


def validate(effects):
    """返回错误描述列表，空列表代表可以保存。"""
    errs = []
    for i, e in enumerate(effects):
        name = e["pic_name"].strip() or ("第 %d 条" % (i + 1))
        if e["rarity"] <= 0:
            errs.append("%s：稀有度必须大于 0（权重是 1/稀有度）" % name)
        if e["circle"] < 0:
            errs.append("%s：圈层不能为负" % name)
    return errs


def warnings(effects, sprite_keys):
    """不挡保存，只在状态栏提示的那种问题。"""
    out = []
    for i, e in enumerate(effects):
        name = e["pic_name"].strip() or ("第 %d 条" % (i + 1))
        if not e["pic_name"].strip():
            out.append("%s：没填图标名" % name)
        elif e["pic_name"].strip().lower() not in sprite_keys:
            out.append("%s：Sprite 目录里找不到 %s.png" % (name, e["pic_name"]))
        if not e["needs"]:
            out.append("%s：没有任何激活条件" % name)
        if not e["effects"]:
            out.append("%s：没有任何效果" % name)
    return out


def selftest():
    effects, created = load_effects()
    print("LocationEffect.json：%d 条%s" % (len(effects), "（新建）" if created else ""))
    print("字段：%s / needs:%s / effects:%s"
          % (", ".join(EFFECT_KEYS), ",".join(NEED_KEYS), ",".join(CAR_EFFECT_KEYS)))

    counts = ring_counts(GRID_W, GRID_H)
    print("圈层 %d×%d：%s" % (GRID_W, GRID_H,
                              "  ".join("圈%d=%d格" % kv for kv in sorted(counts.items()))))
    assert sum(counts.values()) == GRID_W * GRID_H, "圈层没有覆盖满整个网格"
    assert counts == {0: 3, 1: 12, 2: 20}, "7×5 的圈层应该是 3 / 12 / 20"
    assert ring_target(0.4, GRID_W, GRID_H) == 0, "0.4 应落在圈 0"
    assert ring_target(1.6, GRID_W, GRID_H) == 2, "1.6 应落在圈 2"
    assert ring_target(9.0, GRID_W, GRID_H) == 2, "超出最大圈要夹到最外圈"
    assert ring_target(-3.0, GRID_W, GRID_H) == 0, "负数要夹到圈 0"
    print("圈层换算通过")

    probe = [make_effect(), make_effect()]
    probe[0]["rarity"] = 1.0
    probe[1]["rarity"] = 2.0
    ps = probabilities(probe)
    print("稀有度 1 / 2 → %.1f%% / %.1f%%" % (ps[0], ps[1]))
    assert abs(sum(ps) - 100.0) < 1e-6, "概率没归一"
    assert abs(ps[0] - 200.0 / 3) < 1e-6, "稀有度概率算错"
    assert probabilities([make_effect(), make_effect()])[0] == 50.0, "同稀有度应该各占一半"

    before = json.dumps(effects, ensure_ascii=False, sort_keys=True)
    fd, tmp = tempfile.mkstemp(suffix=".json")
    os.close(fd)
    try:
        doc = {"effects": [{k: e[k] for k in EFFECT_KEYS} for e in effects]}
        with open(tmp, "w", encoding="utf-8", newline="\n") as f:
            json.dump(doc, f, ensure_ascii=False, indent=2)
        with open(tmp, encoding="utf-8") as f:
            back = json.load(f)["effects"]
        assert before == json.dumps(back, ensure_ascii=False, sort_keys=True), "写盘再回读不一致"
        print("读写往返一致")
    finally:
        os.remove(tmp)

    ce = make_car_effect(4, 12.5, True)
    assert list(ce.keys()) == CAR_EFFECT_KEYS, "CarEffect 字段顺序要和 LocationEffect.CarEffect 对齐"
    assert "永久" in car_effect_text(ce), "forever 没显示出来"
    assert list(make_need(2, 15).keys()) == NEED_KEYS, "ActNeeded 字段顺序不对"
    print("子表字段通过：%s" % car_effect_text(ce))

    errs = validate(effects)
    if errs:
        print("校验失败：")
        for e in errs:
            print("  - " + e)
        return 1
    print("校验通过")
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

WALL_BG = "#3c3c3c"
CARD_BG = "#fbfbf7"
CARD_BORDER = "#7a7a7a"
CARD_SELECTED = "#2f8fff"
CARD_W, CARD_H = 170, 178


def sprite_index():
    """pic_name(小写) -> png 路径，兼容 Engine/engine.png 这种大小写不一致。"""
    idx = {}
    if SPRITE_DIR.is_dir():
        for p in SPRITE_DIR.iterdir():
            if p.suffix.lower() == ".png":
                idx.setdefault(p.stem.lower(), p)
    return idx


class IconCache:
    """把 Sprite/<pic_name>.png 缩成指定尺寸的 PhotoImage，按 (pic_name,size) 缓存。"""

    def __init__(self):
        self.cache = {}
        self.index = sprite_index()

    def get(self, pic_name, size):
        key = ((pic_name or "").lower(), size)
        if key in self.cache:
            return self.cache[key]
        img = None
        path = self.index.get(key[0]) if pic_name else None
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
        self.effects, created = load_effects()
        self.sel = None
        self.dirty = False
        self.icons = IconCache()
        self.sprite_keys = sorted(self.icons.index.keys(), key=str.lower)
        self.sprite_names = [self.icons.index[k].stem for k in self.sprite_keys]
        self.needs = []
        self.car_effects = []
        self.card_widgets = {}
        self.cols_now = 0

        root.title("位置效果编辑器 — %s" % LOCATION_EFFECT_PATH.name)
        root.geometry("1240x820")
        root.minsize(980, 600)

        style = ttk.Style()
        try:
            style.theme_use("vista")
        except Exception:
            pass
        style.configure("TButton", font=UI_FONT)

        self._build_toolbar()
        self._build_body()
        self.clear_form()
        self.refresh_wall()
        if self.effects:
            self.select(0)
        self.update_status("已建空表" if created else "")

        root.protocol("WM_DELETE_WINDOW", self.on_close)
        root.bind("<Control-s>", lambda e: self.save())
        root.after(120, self._init_sash)

    def _init_sash(self):
        try:
            self.paned.sashpos(0, 780)
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
        btn("新增效果", self.add_effect, 9)
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
        self.canvas.bind("<Button-1>", lambda e: self.select(None))

        right = ttk.Frame(self.paned)
        self.paned.add(right, weight=4)
        self._build_form(right)


    # ---------------- 效果墙
    def _on_canvas_configure(self, event):
        self.canvas.itemconfigure("wall", width=event.width)
        if self._cols() != self.cols_now:
            self.refresh_wall()

    def _cols(self):
        width = self.canvas.winfo_width() or CARD_W * 3
        return max(1, width // (CARD_W + 12))

    def _match(self, e):
        text = self.filter_var.get().strip().lower()
        if not text:
            return True
        return text in e["pic_name"].lower() or text in e["describe"].lower()

    def refresh_wall(self, keep_scroll=True):
        y = self.canvas.yview()[0] if keep_scroll else 0.0
        for w in self.wall.winfo_children():
            w.destroy()
        self.card_widgets.clear()

        cols = self._cols()
        self.cols_now = cols
        row = col = 0
        pcts = probabilities(self.effects)
        for i, e in enumerate(self.effects):
            if not self._match(e):
                continue
            self._make_card(i, e, pcts[i]).grid(row=row, column=col, padx=6, pady=6, sticky="nw")
            col += 1
            if col >= cols:
                col, row = 0, row + 1
        self.wall.update_idletasks()
        self.canvas.configure(scrollregion=self.canvas.bbox("all"))
        self.canvas.yview_moveto(y)
        self._highlight()

    def _make_card(self, i, e, pct):
        frame = tk.Frame(self.wall, width=CARD_W, height=CARD_H, bg=CARD_BG,
                         highlightthickness=3, highlightbackground=CARD_BORDER)
        frame.grid_propagate(False)
        frame.pack_propagate(False)

        top = tk.Frame(frame, bg=CARD_BG)
        top.pack(fill="x", padx=6, pady=(4, 0))
        tk.Label(top, text="%.1f%%" % pct, font=UI_FONT_B, bg=CARD_BG, fg="#b35c00").pack(side="left")
        dele = tk.Label(top, text="✕", font=UI_FONT_S, bg=CARD_BG, fg="#999", cursor="hand2")
        dele.pack(side="right")
        dele.bind("<Button-1>", lambda ev, idx=i: self.delete_one(idx))

        name = tk.Label(frame, text=e["pic_name"] or "(未填图标)", font=UI_FONT_B, bg=CARD_BG,
                        fg="#111", wraplength=CARD_W - 18, justify="center")
        name.pack(side="bottom", pady=(0, 6))
        sub = tk.Label(frame, text=self._sub_text(e), font=UI_FONT_S, bg=CARD_BG, fg="#8a8a8a")
        sub.pack(side="bottom", pady=(0, 2))

        img = self.icons.get(e["pic_name"], 62)
        icon = tk.Label(frame, image=img or "", bg=CARD_BG)
        icon.image = img
        icon.pack(side="top", expand=True)

        for w in (frame, top, name, sub, icon):
            w.bind("<Button-1>", lambda ev, idx=i: self.select(idx))
            w.bind("<MouseWheel>", self._on_wheel)
        self.card_widgets[i] = frame
        return frame

    @staticmethod
    def _sub_text(e):
        return "稀有度 %g · 圈 %g · 条件 %d · 效果 %d" % (
            e["rarity"], e["circle"], len(e["needs"]), len(e["effects"]))

    def _on_wheel(self, event):
        self.canvas.yview_scroll(int(-event.delta / 120), "units")

    def _highlight(self):
        for i, frame in self.card_widgets.items():
            frame.configure(highlightbackground=CARD_SELECTED if i == self.sel else CARD_BORDER)


    # ---------------- 单条编辑面板
    def _build_form(self, parent):
        box = ttk.Frame(parent, padding=(10, 8))
        box.pack(fill="both", expand=True)
        box.columnconfigure(1, weight=1)
        r = 0

        def row(label, widget, hint=""):
            nonlocal r
            ttk.Label(box, text=label, font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
            widget.grid(row=r, column=1, sticky="we", pady=3)
            r += 1
            if hint:
                ttk.Label(box, text=hint, foreground="#888", font=UI_FONT_S).grid(
                    row=r, column=1, sticky="w", pady=(0, 2))
                r += 1
            return widget

        picrow = ttk.Frame(box)
        self.pic_var = tk.StringVar()
        cb = ttk.Combobox(picrow, textvariable=self.pic_var, values=self.sprite_names, font=UI_FONT)
        cb.pack(side="left", fill="x", expand=True)
        cb.bind("<<ComboboxSelected>>", lambda ev: self.apply_live())
        cb.bind("<KeyRelease>", lambda ev: self.apply_live())
        self.form_icon = tk.Label(picrow, bg=CARD_BG)
        self.form_icon.pack(side="left", padx=(8, 0))
        row("图标 pic_name", picrow, "Sprite 目录里的名字，对应 Sprite/<pic_name>.png")

        numrow = ttk.Frame(box)
        self.rarity_var = tk.StringVar()
        e1 = ttk.Entry(numrow, textvariable=self.rarity_var, width=10)
        e1.pack(side="left")
        e1.bind("<KeyRelease>", lambda ev: self.apply_live())
        ttk.Label(numrow, text="  权重 = 1/稀有度", foreground="#888", font=UI_FONT_S).pack(side="left")
        row("稀有度 rarity", numrow, "float，随便写小数；卡片上的百分比就是按它算的")

        self.circle_var = tk.StringVar()
        e2 = ttk.Entry(box, textvariable=self.circle_var)
        e2.bind("<KeyRelease>", lambda ev: self.apply_live())
        row("圈层 circle", e2, "float，位置正态的 µ：0 贴中心，2 贴最外圈")

        ttk.Label(box, text="描述", font=UI_FONT).grid(row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        dbox = ttk.Frame(box)
        dbox.grid(row=r, column=1, sticky="nsew", pady=3)
        self.t_desc = tk.Text(dbox, height=3, wrap="word", font=UI_FONT, undo=True)
        self.t_desc.pack(side="left", fill="both", expand=True)
        ttk.Scrollbar(dbox, orient="vertical", command=self.t_desc.yview).pack(side="right", fill="y")
        self.t_desc.bind("<KeyRelease>", lambda e: self.apply_live())
        r += 1

        ttk.Label(box, text="激活条件 needs", font=UI_FONT).grid(
            row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        nbox = ttk.Frame(box)
        nbox.grid(row=r, column=1, sticky="we", pady=3)
        self.lb_need = tk.Listbox(nbox, height=4, font=UI_FONT)
        self.lb_need.pack(side="left", fill="both", expand=True)
        side = ttk.Frame(nbox)
        side.pack(side="left", fill="y", padx=(6, 0))
        for text, cmd in (("添加", self.need_add), ("编辑", self.need_edit), ("删除", self.need_del)):
            ttk.Button(side, text=text, width=5, command=cmd).pack(pady=1)
        r += 1

        ttk.Label(box, text="效果 effects", font=UI_FONT).grid(
            row=r, column=0, sticky="nw", pady=3, padx=(0, 6))
        cbox = ttk.Frame(box)
        cbox.grid(row=r, column=1, sticky="we", pady=3)
        self.lb_eff = tk.Listbox(cbox, height=4, font=UI_FONT)
        self.lb_eff.pack(side="left", fill="both", expand=True)
        side2 = ttk.Frame(cbox)
        side2.pack(side="left", fill="y", padx=(6, 0))
        for text, cmd in (("添加", self.eff_add), ("编辑", self.eff_edit), ("删除", self.eff_del)):
            ttk.Button(side2, text=text, width=5, command=cmd).pack(pady=1)
        r += 1

        self._build_ring(box, r)

    # ---------------- 圈层预览
    def _build_ring(self, parent, r):
        frame = ttk.LabelFrame(parent, text="圈层预览（中间三个 = 圈 0，每圈往四周各扩 1 格）", padding=8)
        frame.grid(row=r, column=0, columnspan=2, sticky="we", pady=(10, 0))

        size = ttk.Frame(frame)
        size.pack(anchor="w")
        ttk.Label(size, text="网格", font=UI_FONT).pack(side="left")
        self.grid_w_var = tk.StringVar(value=str(GRID_W))
        self.grid_h_var = tk.StringVar(value=str(GRID_H))
        for var in (self.grid_w_var, self.grid_h_var):
            sb = tk.Spinbox(size, from_=1, to=15, width=4, textvariable=var, font=UI_FONT,
                            command=self.draw_ring)
            sb.pack(side="left", padx=(4, 10))
            sb.bind("<KeyRelease>", lambda e: self.draw_ring())

        self.ring_canvas = tk.Canvas(frame, width=300, height=190, bg="#ffffff", highlightthickness=1)
        self.ring_canvas.pack(anchor="w", pady=(6, 0))
        self.ring_info = ttk.Label(frame, text="", font=UI_FONT_S, foreground="#666", justify="left")
        self.ring_info.pack(anchor="w", pady=(6, 0))

    def grid_size(self):
        w = max(1, min(15, to_int(self.grid_w_var.get(), GRID_W)))
        h = max(1, min(15, to_int(self.grid_h_var.get(), GRID_H)))
        return w, h

    def draw_ring(self):
        c = self.ring_canvas
        c.delete("all")
        w, h = self.grid_size()
        cell = max(12, min(28, 300 // w))
        circle = to_float(self.circle_var.get(), 0.0)
        target = ring_target(circle, w, h)
        palette = ["#ffd79a", "#e8c37c", "#c9a763", "#a98b57", "#8a7048"]
        for (x, y), k in ring_cells(w, h).items():
            px, py = x * cell + 2, (h - 1 - y) * cell + 2
            fill = "#ff8a00" if k == target else palette[min(k, len(palette) - 1)]
            c.create_rectangle(px, py, px + cell, py + cell, fill=fill,
                               outline="#ff2fd0" if k == target else "#ffffff",
                               width=2 if k == target else 1)
        counts = ring_counts(w, h)
        self.ring_info.configure(text="圈层格数：" + "   ".join(
            "圈%d=%d格" % kv for kv in sorted(counts.items())) +
            "\n当前 circle=%g → 落圈 %d（%d 格）" % (circle, target, counts.get(target, 0)))


    # ---------------- 表单读写
    def select(self, idx):
        self.flush_form()
        if idx is None or not (0 <= idx < len(self.effects)):
            self.sel = None
            self.clear_form()
        else:
            self.sel = idx
            self.fill_form(idx)
        self._highlight()
        self.update_status()

    @staticmethod
    def _set_text(widget, value):
        widget.delete("1.0", "end")
        widget.insert("1.0", value or "")

    def fill_form(self, idx):
        e = self.effects[idx]
        self.pic_var.set(e["pic_name"])
        self.rarity_var.set("%g" % e["rarity"])
        self.circle_var.set("%g" % e["circle"])
        self._set_text(self.t_desc, e["describe"])
        self.needs = [dict(n) for n in e["needs"]]
        self.car_effects = [dict(c) for c in e["effects"]]
        self.refresh_lists()
        self.refresh_icon()
        self.draw_ring()

    def clear_form(self):
        self.pic_var.set("")
        self.rarity_var.set("1")
        self.circle_var.set("0")
        self._set_text(self.t_desc, "")
        self.needs = []
        self.car_effects = []
        self.refresh_lists()
        self.refresh_icon()
        self.draw_ring()

    def refresh_icon(self):
        img = self.icons.get(self.pic_var.get().strip(), 44)
        self.form_icon.configure(image=img or "")
        self.form_icon.image = img

    def refresh_lists(self):
        self.lb_need.delete(0, "end")
        for n in self.needs:
            self.lb_need.insert("end", need_text(n))
        self.lb_eff.delete(0, "end")
        for c in self.car_effects:
            self.lb_eff.insert("end", car_effect_text(c))

    def flush_form(self):
        if self.sel is None or not (0 <= self.sel < len(self.effects)):
            return
        e = self.effects[self.sel]
        old = json.dumps(e, ensure_ascii=False, sort_keys=True)
        e["pic_name"] = self.pic_var.get().strip()
        e["rarity"] = to_float(self.rarity_var.get(), 1.0)
        e["circle"] = to_float(self.circle_var.get(), 0.0)
        e["describe"] = self.t_desc.get("1.0", "end-1c")
        e["needs"] = [dict(n) for n in self.needs]
        e["effects"] = [dict(c) for c in self.car_effects]
        if json.dumps(e, ensure_ascii=False, sort_keys=True) != old:
            self.dirty = True

    def apply_live(self):
        self.flush_form()
        self.refresh_wall()
        self.refresh_icon()
        self.draw_ring()
        self.update_status()

    # ---------------- needs / effects 子表
    def pick_item(self, title, labels, kind_index, number, number_label, flag_label=None, flag=False):
        """needs / effects 的通用小弹窗，返回 (下标, 数值, 开关) 或 None。"""
        win = tk.Toplevel(self.root)
        win.title(title)
        win.transient(self.root)
        win.resizable(False, False)
        body = ttk.Frame(win, padding=12)
        body.pack(fill="both", expand=True)

        idx_var = tk.StringVar(value=labels[kind_index] if 0 <= kind_index < len(labels) else labels[0])
        ttk.Combobox(body, textvariable=idx_var, values=labels, state="readonly",
                     font=UI_FONT, width=34).grid(row=0, column=0, columnspan=2, sticky="we", pady=4)
        num_var = tk.StringVar(value="%g" % number)
        ttk.Label(body, text=number_label, font=UI_FONT).grid(row=1, column=0, sticky="w", pady=4)
        ttk.Entry(body, textvariable=num_var, width=12).grid(row=1, column=1, sticky="w", pady=4)

        flag_var = tk.BooleanVar(value=bool(flag))
        if flag_label:
            ttk.Checkbutton(body, text=flag_label, variable=flag_var).grid(
                row=2, column=0, columnspan=2, sticky="w", pady=4)

        result = {}

        def ok():
            result["value"] = (labels.index(idx_var.get()), to_float(num_var.get(), 0.0), flag_var.get())
            win.destroy()

        bar = ttk.Frame(body)
        bar.grid(row=3, column=0, columnspan=2, sticky="e", pady=(10, 0))
        ttk.Button(bar, text="确定", width=7, command=ok).pack(side="left", padx=4)
        ttk.Button(bar, text="取消", width=7, command=win.destroy).pack(side="left")

        win.grab_set()
        self.root.wait_window(win)
        return result.get("value")

    def need_add(self):
        got = self.pick_item("新增激活条件", NEEDED_VALUES, 0, 0.0, "参数 needpara")
        if got:
            self.needs.append(make_need(got[0], got[1]))
            self.refresh_lists()
            self.apply_live()

    def need_edit(self):
        sel = self.lb_need.curselection()
        if not sel:
            return
        n = self.needs[sel[0]]
        got = self.pick_item("编辑激活条件", NEEDED_VALUES, n["need"], n["needpara"], "参数 needpara")
        if got:
            self.needs[sel[0]] = make_need(got[0], got[1])
            self.refresh_lists()
            self.apply_live()

    def need_del(self):
        sel = self.lb_need.curselection()
        if sel:
            del self.needs[sel[0]]
            self.refresh_lists()
            self.apply_live()

    def eff_add(self):
        got = self.pick_item("新增效果", EFFECTKIND_VALUES, 0, 0.0, "数值 effectNum", "永久 (forever)")
        if got:
            self.car_effects.append(make_car_effect(got[0], got[1], got[2]))
            self.refresh_lists()
            self.apply_live()

    def eff_edit(self):
        sel = self.lb_eff.curselection()
        if not sel:
            return
        c = self.car_effects[sel[0]]
        got = self.pick_item("编辑效果", EFFECTKIND_VALUES, c["effectKind"], c["effectNum"],
                             "数值 effectNum", "永久 (forever)", c.get("forever", False))
        if got:
            self.car_effects[sel[0]] = make_car_effect(got[0], got[1], got[2])
            self.refresh_lists()
            self.apply_live()

    def eff_del(self):
        sel = self.lb_eff.curselection()
        if sel:
            del self.car_effects[sel[0]]
            self.refresh_lists()
            self.apply_live()

    # ---------------- 命令
    def add_effect(self):
        self.flush_form()
        self.effects.append(make_effect())
        self.dirty = True
        self.refresh_wall()
        self.select(len(self.effects) - 1)
        self.update_status("已新增一条位置效果")

    def delete_one(self, i):
        if not messagebox.askyesno("删除", "确定删除第 %d 条位置效果？" % (i + 1)):
            return
        del self.effects[i]
        self.sel = None
        self.dirty = True
        self.refresh_wall()
        if self.effects:
            self.select(min(i, len(self.effects) - 1))
        else:
            self.clear_form()
            self._highlight()
        self.update_status("已删除 1 条")

    def delete_selection(self):
        if self.sel is None:
            self.update_status("先选一条再删")
            return
        self.delete_one(self.sel)

    def save(self):
        self.flush_form()
        errs = validate(self.effects)
        if errs:
            messagebox.showwarning("无法保存", "请先修正以下问题：\n\n" + "\n".join(errs[:12]) +
                                   ("\n…" if len(errs) > 12 else ""))
            self.update_status("校验失败 %d 项" % len(errs))
            return False
        try:
            save_effects(self.effects)
        except Exception as exc:
            messagebox.showerror("保存失败", str(exc))
            return False
        self.dirty = False
        self.update_status("已保存 %s（%d 条）" % (LOCATION_EFFECT_PATH.name, len(self.effects)))
        return True

    def reload(self):
        if self.dirty and not messagebox.askyesno("重新载入", "有未保存的改动，确定丢弃？"):
            return
        self.effects, _ = load_effects()
        self.sel = None
        self.dirty = False
        self.icons = IconCache()
        self.sprite_keys = sorted(self.icons.index.keys(), key=str.lower)
        self.sprite_names = [self.icons.index[k].stem for k in self.sprite_keys]
        self.refresh_wall()
        if self.effects:
            self.select(0)
        else:
            self.clear_form()
            self._highlight()
        self.update_status("已重新载入（%d 条）" % len(self.effects))

    def open_folder(self):
        try:
            os.startfile(str(DATA_DIR))
        except Exception as exc:
            messagebox.showerror("打不开目录", str(exc))

    def update_status(self, extra=""):
        mark = "● 未保存" if self.dirty else "已保存"
        text = "位置效果 %d 条 | Sprite %d 个 | %s" % (len(self.effects), len(self.sprite_names), mark)
        if self.sel is not None:
            text += " | 选中第 %d 条" % (self.sel + 1)
        warns = warnings(self.effects, set(self.sprite_keys))
        if warns:
            text += " | 提示：%s" % warns[0] + (" 等 %d 项" % len(warns) if len(warns) > 1 else "")
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
        app.add_effect()
        root.update_idletasks()
        counts = ring_counts(GRID_W, GRID_H)
        print("UI OK：位置效果 %d 条，卡片墙格子 %d 个，列数 %d，圈层 %s"
              % (len(app.effects), len(app.card_widgets), app._cols(),
                 "  ".join("圈%d=%d格" % kv for kv in sorted(counts.items()))))
        root.destroy()
        return 0
    root.mainloop()
    return 0


if __name__ == "__main__":
    sys.exit(main())
