from __future__ import annotations

import math
from pathlib import Path
from xml.sax.saxutils import escape

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "rendered"
OUT.mkdir(parents=True, exist_ok=True)

FONT_REGULAR = Path(r"C:\Windows\Fonts\arial.ttf")
FONT_BOLD = Path(r"C:\Windows\Fonts\arialbd.ttf")

INK = "#111827"
LINE = "#475569"
MUTED = "#64748B"
ACCENT = "#2563EB"
HEADER = "#EAF2FF"
SOFT = "#F8FAFC"
WARN = "#FFF7ED"
ERROR = "#FEF2F2"
SUCCESS = "#F0FDF4"
WHITE = "#FFFFFF"


def font(size: int, bold: bool = False):
    return ImageFont.truetype(str(FONT_BOLD if bold else FONT_REGULAR), size)


class Canvas:
    def __init__(self, width: int, height: int, title: str, subtitle: str = ""):
        self.width = width
        self.height = height
        self.img = Image.new("RGB", (width, height), WHITE)
        self.draw = ImageDraw.Draw(self.img)
        self.svg: list[str] = [
            f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">',
            '<defs><marker id="arrow" markerWidth="12" markerHeight="12" refX="10" refY="6" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L12,6 L0,12 z" fill="#475569"/></marker></defs>',
            f'<rect width="{width}" height="{height}" fill="{WHITE}"/>',
        ]
        self.text(width / 2, 34, title, 34, bold=True, anchor="middle")
        if subtitle:
            self.text(width / 2, 78, subtitle, 20, fill=MUTED, anchor="middle")

    def text(self, x, y, value, size=22, *, bold=False, fill=INK, anchor="start"):
        fnt = font(size, bold)
        if anchor == "middle":
            box = self.draw.textbbox((0, 0), value, font=fnt)
            px = x - (box[2] - box[0]) / 2
            svg_anchor = "middle"
        elif anchor == "end":
            box = self.draw.textbbox((0, 0), value, font=fnt)
            px = x - (box[2] - box[0])
            svg_anchor = "end"
        else:
            px = x
            svg_anchor = "start"
        self.draw.text((px, y), value, font=fnt, fill=fill)
        family = "Arial"
        weight = "700" if bold else "400"
        baseline = y + size
        self.svg.append(
            f'<text x="{x}" y="{baseline}" text-anchor="{svg_anchor}" font-family="{family}" '
            f'font-size="{size}" font-weight="{weight}" fill="{fill}">{escape(value)}</text>'
        )

    def multiline(self, x, y, lines, size=21, *, bold=False, fill=INK, anchor="start", gap=7):
        line_h = size + gap
        for i, line in enumerate(lines):
            self.text(x, y + i * line_h, line, size, bold=bold, fill=fill, anchor=anchor)

    def rect(self, x, y, w, h, *, fill=WHITE, outline=LINE, radius=16, width=3):
        self.draw.rounded_rectangle((x, y, x + w, y + h), radius=radius, fill=fill, outline=outline, width=width)
        self.svg.append(
            f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{radius}" fill="{fill}" stroke="{outline}" stroke-width="{width}"/>'
        )

    def line(self, points, *, fill=LINE, width=4, arrow=True, dash=None):
        self.draw.line(points, fill=fill, width=width, joint="curve")
        if arrow and len(points) >= 2:
            (x1, y1), (x2, y2) = points[-2], points[-1]
            ang = math.atan2(y2 - y1, x2 - x1)
            length = 17
            spread = 7
            p1 = (x2, y2)
            p2 = (x2 - length * math.cos(ang) + spread * math.sin(ang), y2 - length * math.sin(ang) - spread * math.cos(ang))
            p3 = (x2 - length * math.cos(ang) - spread * math.sin(ang), y2 - length * math.sin(ang) + spread * math.cos(ang))
            self.draw.polygon([p1, p2, p3], fill=fill)
        pts = " ".join(f"{x},{y}" for x, y in points)
        marker = ' marker-end="url(#arrow)"' if arrow else ""
        dash_attr = f' stroke-dasharray="{dash}"' if dash else ""
        self.svg.append(f'<polyline points="{pts}" fill="none" stroke="{fill}" stroke-width="{width}" stroke-linejoin="round"{marker}{dash_attr}/>' )

    def circle(self, x, y, r, *, fill=WHITE, outline=LINE, width=3):
        self.draw.ellipse((x-r, y-r, x+r, y+r), fill=fill, outline=outline, width=width)
        self.svg.append(f'<circle cx="{x}" cy="{y}" r="{r}" fill="{fill}" stroke="{outline}" stroke-width="{width}"/>')

    def diamond(self, cx, cy, w, h, *, fill=WARN, outline=LINE, width=3):
        pts = [(cx, cy-h/2), (cx+w/2, cy), (cx, cy+h/2), (cx-w/2, cy)]
        self.draw.polygon(pts, fill=fill, outline=outline)
        self.draw.line(pts + [pts[0]], fill=outline, width=width)
        p = " ".join(f"{x},{y}" for x, y in pts)
        self.svg.append(f'<polygon points="{p}" fill="{fill}" stroke="{outline}" stroke-width="{width}"/>')

    def save(self, stem: str):
        self.svg.append("</svg>")
        (OUT / f"{stem}.svg").write_text("\n".join(self.svg), encoding="utf-8")
        self.img.save(OUT / f"{stem}.png", optimize=True)


def entity(c: Canvas, x, y, w, title, table, rows, *, header=HEADER):
    header_h = 76
    row_h = 37
    h = header_h + row_h * len(rows) + 20
    c.rect(x, y, w, h, fill=WHITE, radius=12, width=3)
    c.draw.rounded_rectangle((x, y, x+w, y+header_h), radius=12, fill=header, outline=LINE, width=3)
    c.draw.rectangle((x, y+header_h-12, x+w, y+header_h), fill=header)
    c.svg.append(f'<path d="M{x+12},{y} H{x+w-12} Q{x+w},{y} {x+w},{y+12} V{y+header_h} H{x} V{y+12} Q{x},{y} {x+12},{y} Z" fill="{header}" stroke="{LINE}" stroke-width="3"/>')
    c.text(x+w/2, y+12, title, 24, bold=True, anchor="middle")
    c.text(x+w/2, y+42, table, 19, fill=MUTED, anchor="middle")
    c.line([(x, y+header_h), (x+w, y+header_h)], width=2, arrow=False)
    for i, row in enumerate(rows):
        c.text(x+18, y+header_h+10+i*row_h, row, 18)
    return (x, y, w, h)


def process(c: Canvas, x, y, w, h, code, lines, *, fill=WHITE):
    c.rect(x, y, w, h, fill=fill, radius=22, width=4)
    c.text(x+24, y+18, code, 22, bold=True, fill=ACCENT)
    c.multiline(x+w/2, y+52, lines, 23, bold=True, anchor="middle", gap=7)
    return (x, y, w, h)


def external(c: Canvas, x, y, w, h, lines):
    c.rect(x, y, w, h, fill=SOFT, radius=8, width=3)
    c.multiline(x+w/2, y+24, lines, 22, bold=True, anchor="middle", gap=7)
    return (x, y, w, h)


def store(c: Canvas, x, y, w, h, code, lines):
    c.rect(x, y, w, h, fill=WHITE, radius=3, width=3)
    c.line([(x+20, y), (x+20, y+h)], width=3, arrow=False)
    c.text(x+38, y+20, code, 21, bold=True, fill=ACCENT)
    c.multiline(x+w/2+12, y+50, lines, 20, anchor="middle", gap=6)
    return (x, y, w, h)


def architecture_component(c: Canvas, x, y, w, h, title, subtitle, lines=(), *, fill=WHITE, accent=HEADER):
    c.rect(x, y, w, h, fill=fill, radius=18, width=3)
    c.draw.rounded_rectangle((x, y, x+w, y+66), radius=18, fill=accent, outline=LINE, width=3)
    c.draw.rectangle((x, y+48, x+w, y+66), fill=accent)
    c.svg.append(
        f'<path d="M{x+18},{y} H{x+w-18} Q{x+w},{y} {x+w},{y+18} V{y+66} H{x} V{y+18} Q{x},{y} {x+18},{y} Z" '
        f'fill="{accent}" stroke="{LINE}" stroke-width="3"/>'
    )
    c.text(x+w/2, y+14, title, 23, bold=True, anchor="middle")
    c.text(x+w/2, y+76, subtitle, 18, fill=MUTED, anchor="middle")
    if lines:
        c.multiline(x+24, y+112, lines, 18, gap=7)
    return (x, y, w, h)


def architecture_zone(c: Canvas, x, y, w, h, title, *, fill="#FBFDFF"):
    c.rect(x, y, w, h, fill=fill, outline="#94A3B8", radius=22, width=3)
    c.rect(x+18, y-22, max(260, len(title)*13), 48, fill=WHITE, outline=WHITE, radius=0, width=0)
    c.text(x+30, y-12, title, 22, bold=True, fill=ACCENT)
    return (x, y, w, h)


def box_anchor(box, side):
    x, y, w, h = box
    return {
        "left": (x, y+h/2), "right": (x+w, y+h/2),
        "top": (x+w/2, y), "bottom": (x+w/2, y+h)
    }[side]


def connect(c, a, aside, b, bside, label="", *, route=None, dash=None):
    start = box_anchor(a, aside)
    end = box_anchor(b, bside)
    pts = [start] + (route or []) + [end]
    c.line(pts, dash=dash)
    if label:
        if route:
            lx, ly = route[len(route)//2]
        else:
            lx, ly = ((start[0]+end[0])/2, (start[1]+end[1])/2)
        c.rect(lx-92, ly-19, 184, 38, fill=WHITE, outline=WHITE, radius=0, width=0)
        c.text(lx, ly-13, label, 16, fill=MUTED, anchor="middle")


def build_erd():
    c = Canvas(3300, 1900, "ERD cốt lõi HTQLHDK — tích hợp tự động OWASP ZAP", "Các FK người dùng, nhóm, audit và thông báo được lược khỏi hình để giữ rõ luồng nghiệp vụ chính")
    a = entity(c, 60, 410, 390, "Tài sản website", "asset", ["PK  id", "code", "FK  current_scope_id", "status"])
    s = entity(c, 510, 410, 390, "Phiên bản phạm vi", "asset_scope_revision", ["PK  id", "FK  asset_id", "version_no", "status"])
    p = entity(c, 960, 120, 420, "Hồ sơ quét ZAP", "zap_scan_profile", ["PK  id", "code + version_no", "status", "active_scan_enabled", "FK  service_user_id"])
    sch = entity(c, 960, 480, 420, "Lịch quét", "scan_schedule", ["PK  id", "FK  asset_id", "FK  scope_id", "FK  zap_profile_id", "automation_enabled", "next_due_at", "status"])
    sc = entity(c, 1450, 480, 410, "Lần quét nghiệp vụ", "scan_occurrence", ["PK  id", "FK  asset_id", "FK  scope_id", "FK  schedule_id", "purpose", "status"])
    ex = entity(c, 1930, 410, 420, "Phiên thực thi ZAP", "zap_execution", ["PK  id", "FK/UQ  scan_id", "FK  profile_id", "FK/UQ  import_batch_id", "status", "configuration_snapshot"])
    ar = entity(c, 2420, 170, 390, "Artifact báo cáo", "zap_artifact", ["PK  id", "FK  execution_id", "artifact_kind", "object_key", "sha256"])
    ba = entity(c, 2860, 480, 390, "Lô tiếp nhận", "import_batch", ["PK  id", "FK  asset_id", "FK  scan_id", "file_sha256", "raw_object_key", "status"])

    f = entity(c, 620, 1130, 410, "Phát hiện", "finding", ["PK  id", "FK  asset_id", "fingerprint", "FK  current_review_id", "status"])
    fo = entity(c, 1100, 1050, 400, "Lần xuất hiện", "finding_occurrence", ["PK  id", "FK  finding_id", "FK  scan_id", "observed_at"])
    rv = entity(c, 620, 1510, 410, "Đánh giá ATTT", "finding_review", ["PK  id", "FK  finding_id", "proposed_severity", "status"])
    tk = entity(c, 1580, 1420, 400, "Phiếu khắc phục", "remediation_ticket", ["PK  id", "FK  finding_id", "code", "FK  current_event_id"])
    vrq = entity(c, 2110, 1190, 410, "Yêu cầu xác minh", "verification_request", ["PK  id", "FK  finding_id", "FK  ticket_id", "FK  scope_id", "status"])
    vrs = entity(c, 2700, 1400, 420, "Kết quả xác minh", "verification_result", ["PK  id", "FK  request_id", "FK  scan_id", "FK  batch_id", "outcome"])

    connect(c, a, "right", s, "left", "có nhiều phiên bản")
    connect(c, a, "right", sch, "left", "lập lịch", route=[(720, 360), (920, 360), (920, 600)])
    connect(c, s, "right", sch, "left", "scope đã duyệt")
    connect(c, p, "bottom", sch, "top", "cấu hình")
    connect(c, sch, "right", sc, "left", "tạo kỳ quét")
    connect(c, sc, "right", ex, "left", "1 — 0..1")
    connect(c, p, "right", ex, "top", "snapshot profile", route=[(1650, 250), (2140, 250)])
    connect(c, ex, "right", ar, "bottom", "sinh 1..n", route=[(2380, 520), (2615, 520)])
    connect(c, sc, "right", ba, "left", "tạo lô tiếp nhận", route=[(1900, 830), (2800, 830)])
    connect(c, ex, "right", ba, "left", "import_batch_id", route=[(2600, 650)])

    connect(c, a, "bottom", f, "left", "sở hữu", route=[(255, 990), (560, 990), (560, 1250)])
    connect(c, f, "right", fo, "left", "tái xuất hiện")
    connect(c, sc, "bottom", fo, "top", "ghi nhận")
    connect(c, f, "bottom", rv, "top", "đánh giá")
    connect(c, f, "right", tk, "left", "khắc phục", route=[(1300, 1320), (1300, 1530)])
    connect(c, tk, "right", vrq, "left", "sau khắc phục", route=[(2040, 1530), (2040, 1320)])
    connect(c, vrq, "right", vrs, "left", "kết quả")
    connect(c, ba, "bottom", vrs, "top", "bằng chứng batch", route=[(3055, 1080), (2910, 1080)])
    c.save("01_erd_tich_hop_zap_tu_dong")


def build_activity():
    c = Canvas(2400, 2600, "Sơ đồ hoạt động — quy trình quét và khắc phục tự động", "Luồng chuẩn và các nhánh kiểm soát ngoại lệ")
    lane_w = 570
    lane_x = [40, 625, 1210, 1795]
    lane_names = ["Chủ tài sản / ATTT", "Vận hành / Khắc phục", "Bộ lập lịch & điều phối", "OWASP ZAP"]
    for x, name in zip(lane_x, lane_names):
        c.rect(x, 110, lane_w, 2430, fill=WHITE, outline="#CBD5E1", radius=0, width=2)
        c.draw.rectangle((x, 110, x+lane_w, 170), fill=HEADER, outline="#CBD5E1", width=2)
        c.svg.append(f'<rect x="{x}" y="110" width="{lane_w}" height="60" fill="{HEADER}" stroke="#CBD5E1" stroke-width="2"/>')
        c.text(x+lane_w/2, 124, name, 22, bold=True, anchor="middle")

    start = (325, 215)
    c.circle(*start, 18, fill=INK, outline=INK)
    steps = []
    def act(lane, y, lines, *, fill=WHITE, h=92):
        x = lane_x[lane] + 70
        box = (x, y, lane_w-140, h)
        c.rect(*box, fill=fill, radius=18, width=3)
        c.multiline(x+(lane_w-140)/2, y+18, lines, 20, bold=True, anchor="middle", gap=6)
        steps.append(box)
        return box

    a1 = act(0, 270, ["Khai báo tài sản, phạm vi", "và khung giờ quét"])
    a2 = act(0, 410, ["Xác nhận và phê duyệt", "tài sản / phạm vi"])
    a3 = act(1, 550, ["Tạo hồ sơ quét ZAP", "có phiên bản"])
    a4 = act(0, 690, ["Phê duyệt hồ sơ", "và kích hoạt lịch"])
    a5 = act(2, 830, ["Phát hiện lịch đến hạn", "và tạo scan_occurrence"])
    d1c = (lane_x[2]+lane_w/2, 1050)
    c.diamond(*d1c, 360, 150)
    c.multiline(d1c[0], d1c[1]-34, ["Asset, scope, profile", "và khung giờ hợp lệ?"], 18, bold=True, anchor="middle", gap=4)
    fail1 = act(1, 980, ["Ghi Missed/Failed", "và rà soát ngoại lệ"], fill=ERROR, h=100)
    a6 = act(2, 1180, ["Tạo zap_execution", "và snapshot cấu hình"])
    a7 = act(3, 1320, ["Chạy spider, passive scan", "và active scan nếu được duyệt"], h=110)
    d2c = (lane_x[3]+lane_w/2, 1545)
    c.diamond(*d2c, 330, 140)
    c.multiline(d2c[0], d2c[1]-30, ["Hoàn tất", "không lỗi / timeout?"], 18, bold=True, anchor="middle", gap=4)
    fail2 = act(1, 1490, ["Đánh dấu Failed / Timed Out", "hoặc Cancelled; xử lý ngoại lệ"], fill=ERROR, h=110)
    a8 = act(2, 1690, ["Thu báo cáo, kiểm SHA-256,", "định dạng, nguồn và phạm vi"], h=110)
    d3c = (lane_x[2]+lane_w/2, 1905)
    c.diamond(*d3c, 330, 140)
    c.multiline(d3c[0], d3c[1]-30, ["Báo cáo", "hợp lệ?"], 19, bold=True, anchor="middle", gap=4)
    fail3 = act(1, 1850, ["Cách ly báo cáo", "và ghi lỗi tiếp nhận"], fill=ERROR, h=100)
    a9 = act(2, 2070, ["Lưu artifact, tạo import_batch,", "chuẩn hóa và chống trùng"], h=110)
    a10 = act(0, 2210, ["Xác nhận Final Severity", "và phân công khắc phục"], h=100)
    a11 = act(1, 2210, ["Sửa lỗi, cập nhật tiến độ", "và bằng chứng"], h=100)
    a12 = act(2, 2360, ["Tạo và điều phối", "lần quét xác minh"], h=100)
    end_box = act(0, 2360, ["Đóng nếu coverage đạt;", "nếu chưa đạt thì mở lại"], fill=SUCCESS, h=100)

    c.line([start, (start[0], a1[1])])
    connect(c, a1, "bottom", a2, "top")
    connect(c, a2, "bottom", a3, "top", route=[(a2[0]+a2[2]/2, 520), (a3[0]+a3[2]/2, 520)])
    connect(c, a3, "bottom", a4, "top", route=[(a3[0]+a3[2]/2, 660), (a4[0]+a4[2]/2, 660)])
    connect(c, a4, "bottom", a5, "top", route=[(a4[0]+a4[2]/2, 800), (a5[0]+a5[2]/2, 800)])
    c.line([box_anchor(a5, "bottom"), (d1c[0], d1c[1]-75)])
    c.line([(d1c[0]-180, d1c[1]), (fail1[0]+fail1[2], d1c[1]), box_anchor(fail1, "right")])
    c.text(d1c[0]-210, d1c[1]-28, "Không", 17, bold=True, fill=MUTED)
    c.line([(d1c[0], d1c[1]+75), box_anchor(a6, "top")])
    c.text(d1c[0]+20, d1c[1]+85, "Có", 17, bold=True, fill=MUTED)
    connect(c, a6, "bottom", a7, "top", route=[(a6[0]+a6[2]/2, 1290), (a7[0]+a7[2]/2, 1290)])
    c.line([box_anchor(a7, "bottom"), (d2c[0], d2c[1]-70)])
    c.line([(d2c[0]-165, d2c[1]), (fail2[0]+fail2[2], d2c[1]), box_anchor(fail2, "right")])
    c.text(d2c[0]-200, d2c[1]-28, "Không", 17, bold=True, fill=MUTED)
    c.line([(d2c[0], d2c[1]+70), (d2c[0], 1650), (a8[0]+a8[2]/2, 1650), box_anchor(a8, "top")])
    c.text(d2c[0]+20, d2c[1]+80, "Có", 17, bold=True, fill=MUTED)
    c.line([box_anchor(a8, "bottom"), (d3c[0], d3c[1]-70)])
    c.line([(d3c[0]-165, d3c[1]), (fail3[0]+fail3[2], d3c[1]), box_anchor(fail3, "right")])
    c.text(d3c[0]-200, d3c[1]-28, "Không", 17, bold=True, fill=MUTED)
    c.line([(d3c[0], d3c[1]+70), box_anchor(a9, "top")])
    c.text(d3c[0]+20, d3c[1]+80, "Có", 17, bold=True, fill=MUTED)
    connect(c, a9, "bottom", a10, "top", route=[(a9[0]+a9[2]/2, 2190), (a10[0]+a10[2]/2, 2190)])
    connect(c, a10, "right", a11, "left")
    connect(c, a11, "bottom", a12, "top", route=[(a11[0]+a11[2]/2, 2335), (a12[0]+a12[2]/2, 2335)])
    connect(c, a12, "left", end_box, "right")
    end = (end_box[0]+end_box[2]/2, 2500)
    c.line([box_anchor(end_box, "bottom"), end], arrow=False)
    c.circle(*end, 18, fill=WHITE, outline=INK, width=4)
    c.circle(*end, 10, fill=INK, outline=INK, width=2)
    c.save("02_activity_quy_trinh_tong_the")


def build_state():
    c = Canvas(3000, 1450, "Sơ đồ trạng thái — phiên thực thi ZAP tự động", "zap_execution tách trạng thái kỹ thuật khỏi scan_occurrence")
    labels = [
        ("Queued", ["Chờ worker nhận lease"]),
        ("Preparing Plan", ["Kiểm scope/profile", "Sinh Automation Plan"]),
        ("Starting Runner", ["Khởi động runner", "với image khóa digest"]),
        ("Running", ["Spider / passive /", "active scan được duyệt"]),
        ("Collecting Report", ["Sinh và kiểm", "JSON / HTML"]),
        ("Report Received", ["Lưu artifact +", "import_batch Received"]),
    ]
    boxes = []
    x = 115
    for i, (name, desc) in enumerate(labels):
        w = 330 if i not in (1, 2, 4, 5) else 360
        b = (x, 280, w, 180)
        fill = SUCCESS if name == "Report Received" else WHITE
        c.rect(*b, fill=fill, radius=28, width=4)
        c.text(x+w/2, 310, name, 24, bold=True, anchor="middle")
        c.multiline(x+w/2, 355, desc, 19, fill=MUTED, anchor="middle", gap=6)
        boxes.append(b)
        x += w + 110
    c.circle(55, 370, 18, fill=INK, outline=INK)
    c.line([(73, 370), box_anchor(boxes[0], "left")])
    for a, b in zip(boxes, boxes[1:]):
        connect(c, a, "right", b, "left")
    c.circle(2935, 370, 22, fill=WHITE, outline=INK, width=4)
    c.circle(2935, 370, 12, fill=INK, outline=INK)
    c.line([box_anchor(boxes[-1], "right"), (2913, 370)], arrow=False)

    failed = process(c, 470, 790, 420, 150, "KẾT THÚC LỖI", ["Failed"], fill=ERROR)
    timed = process(c, 1040, 790, 420, 150, "KẾT THÚC QUÁ HẠN", ["Timed Out"], fill=WARN)
    cancelled = process(c, 1710, 790, 420, 150, "KẾT THÚC HỦY", ["Cancelled"], fill=SOFT)

    for idx in (1, 2, 3, 4):
        bx = boxes[idx]
        target = failed if idx in (1, 2, 4) else timed
        tx = box_anchor(target, "top")[0]
        connect(c, bx, "bottom", target, "top", route=[(bx[0]+bx[2]/2, 620), (tx, 620)], dash="10 8")
    connect(c, boxes[2], "bottom", timed, "top", route=[(boxes[2][0]+boxes[2][2]/2, 690), (timed[0]+timed[2]/2, 690)], dash="10 8")
    connect(c, boxes[0], "bottom", cancelled, "top", route=[(boxes[0][0]+boxes[0][2]/2, 720), (cancelled[0]+cancelled[2]/2, 720)], dash="10 8")
    connect(c, boxes[3], "bottom", cancelled, "top", route=[(boxes[3][0]+boxes[3][2]/2, 680), (cancelled[0]+cancelled[2]/2, 680)], dash="10 8")

    c.rect(250, 1080, 2500, 240, fill=SOFT, outline="#CBD5E1", radius=18, width=3)
    c.text(300, 1110, "Ràng buộc chuyển trạng thái", 23, bold=True, fill=ACCENT)
    c.multiline(300, 1160, [
        "• Chỉ Report Received mới liên kết import_batch và hoàn tất scan_occurrence.",
        "• Failed, Timed Out và Cancelled phải có finished_at; lỗi phải có failure_code đã che dữ liệu nhạy cảm.",
        "• Không tự retry một phiên đã bắt đầu gửi request; chạy lại phải tạo phiên mới theo thẩm quyền.",
        "• Lease/heartbeat hết hạn chuyển sang Failed hoặc Timed Out để tránh quét trùng âm thầm.",
    ], 21, gap=10)
    c.save("03_state_zap_execution")


def build_dfd():
    c = Canvas(2800, 1800, "Sơ đồ luồng dữ liệu mức 1 — HTQLHDK tích hợp ZAP tự động", "Bốn tiến trình nghiệp vụ chính; phân quyền, audit và thông báo là kiểm soát xuyên suốt")
    owner = external(c, 60, 190, 330, 110, ["Chủ sở hữu", "tài sản"])
    sec = external(c, 60, 500, 330, 110, ["Chuyên viên", "ATTT"])
    ops = external(c, 60, 820, 330, 110, ["Nhóm vận hành", "tích hợp ZAP"])
    remedy = external(c, 60, 1140, 330, 110, ["Nhóm khắc phục", "kỹ thuật"])
    risk = external(c, 60, 1450, 330, 110, ["Người phê duyệt", "rủi ro"])

    p1 = process(c, 590, 180, 500, 160, "P1", ["Quản lý tài sản", "và phạm vi"])
    p2 = process(c, 590, 550, 500, 170, "P2", ["Lập lịch và", "điều phối ZAP"])
    p3 = process(c, 590, 930, 500, 170, "P3", ["Tiếp nhận, kiểm tra", "và chuẩn hóa báo cáo"])
    p4 = process(c, 590, 1320, 500, 180, "P4", ["Phân tích, khắc phục", "và xác minh"])

    d1 = store(c, 1370, 120, 540, 160, "D1", ["Người dùng, nhóm, phạm vi quyền,", "audit và thông báo"])
    d2 = store(c, 1370, 430, 540, 180, "D2", ["Tài sản, phạm vi,", "hồ sơ ZAP và lịch"])
    d3 = store(c, 1370, 820, 540, 190, "D3", ["Lần quét, phiên ZAP,", "artifact và lô tiếp nhận"])
    d4 = store(c, 1370, 1240, 540, 200, "D4", ["Phát hiện, đánh giá, phiếu,", "bằng chứng và rủi ro"])

    zap = external(c, 2260, 530, 420, 160, ["OWASP ZAP", "Hệ thống bên ngoài"])
    c.text(2470, 730, "Không truy cập trực tiếp cơ sở dữ liệu", 18, bold=True, fill="#B45309", anchor="middle")

    connect(c, owner, "right", p1, "left", "khai báo / xác nhận")
    connect(c, sec, "right", p1, "left", "phê duyệt phạm vi", route=[(480, 555), (480, 260)])
    connect(c, p1, "right", d2, "left", "asset + scope", route=[(1210, 260), (1210, 520)])

    connect(c, sec, "right", p2, "left", "lịch / hồ sơ", route=[(500, 555), (500, 635)])
    connect(c, ops, "right", p2, "left", "cấu hình / ngoại lệ", route=[(520, 875), (520, 675)])
    connect(c, d2, "left", p2, "right", "scope + profile")
    connect(c, p2, "right", zap, "left", "plan / chạy / dừng", route=[(2070, 610)])
    connect(c, zap, "bottom", p2, "right", "tiến độ / báo cáo", route=[(2470, 780), (1160, 780), (1160, 680)])
    connect(c, p2, "right", d3, "left", "scan + execution", route=[(1210, 680), (1210, 915)])

    connect(c, d3, "left", p3, "right", "báo cáo gốc")
    connect(c, p3, "right", d4, "left", "finding chuẩn hóa", route=[(1210, 1015), (1210, 1340)])
    connect(c, p3, "left", ops, "right", "lỗi tiếp nhận", route=[(500, 1015), (500, 875)])

    connect(c, sec, "right", p4, "left", "severity / đóng mở", route=[(460, 555), (460, 1390)])
    connect(c, remedy, "right", p4, "left", "tiến độ / bằng chứng", route=[(500, 1195), (500, 1420)])
    connect(c, risk, "right", p4, "left", "quyết định rủi ro")
    connect(c, p4, "right", d4, "left", "review / ticket / risk")
    connect(c, p4, "top", p2, "bottom", "yêu cầu quét xác minh", route=[(1160, 1210), (1160, 760), (840, 760)])

    c.line([(1640, 280), (1640, 350), (1190, 350), (1190, 1430)], arrow=False, dash="10 8")
    c.text(1190, 365, "Kiểm tra quyền + ghi audit cho P1–P4", 18, bold=True, fill=ACCENT, anchor="middle")
    for proc in (p1, p2, p3, p4):
        y = proc[1] + proc[3] / 2
        c.line([(1190, y), (proc[0] + proc[2], y)], dash="10 8")
    c.save("04_dfd_level_1")


def build_architecture():
    c = Canvas(
        3300,
        2000,
        "Kiến trúc tổng thể HTQLHDK — tích hợp tự động OWASP ZAP",
        "API xử lý nghiệp vụ; Worker là ranh giới điều phối ZAP; dữ liệu và báo cáo được bảo vệ trong vùng riêng",
    )

    client_zone = architecture_zone(c, 60, 170, 430, 760, "VÙNG NGƯỜI DÙNG")
    edge_zone = architecture_zone(c, 570, 250, 400, 600, "EDGE / DMZ")
    app_zone = architecture_zone(c, 1060, 140, 1050, 1080, "VÙNG ỨNG DỤNG RIÊNG")
    runner_zone = architecture_zone(c, 2200, 140, 1040, 850, "VÙNG ZAP RUNNER CÔ LẬP", fill="#FFFDF8")
    data_zone = architecture_zone(c, 570, 1380, 2670, 480, "VÙNG DỮ LIỆU VÀ DỊCH VỤ BẢO MẬT", fill="#FAFCFF")

    users = architecture_component(
        c, 105, 260, 340, 220, "Người dùng nghiệp vụ", "Truy cập theo vai trò và tài sản",
        ["• Quản trị viên", "• Chuyên viên ATTT", "• Chủ tài sản", "• Vận hành / Khắc phục / Rủi ro"],
        fill=SOFT,
    )
    browser = architecture_component(
        c, 105, 600, 340, 180, "Trình duyệt", "HTTPS",
        ["Cookie HttpOnly / Secure", "Không kết nối trực tiếp CSDL"],
    )
    edge = architecture_component(
        c, 620, 410, 300, 260, "Reverse proxy / WAF", "Điểm vào được công bố",
        ["TLS termination", "Rate và size limit", "Correlation ID", "Không công bố cổng quản trị"],
    )

    ui = architecture_component(
        c, 1130, 220, 420, 210, "Web UI", "React + TypeScript",
        ["Hiển thị dữ liệu đã masking", "Route guard chỉ hỗ trợ UX", "Không lưu token ở localStorage"],
    )
    api = architecture_component(
        c, 1620, 220, 420, 510, "Backend API", "ASP.NET Core .NET 10",
        [
            "Identity & Authorization",
            "Asset / Scope (F-01)",
            "Scheduling API (F-02)",
            "Import / Finding (F-03)",
            "Remediation / Risk (F-04)",
            "File / Export / Masking",
            "Audit / Notification",
            "Không giữ Docker socket",
        ],
    )
    worker = architecture_component(
        c, 1130, 790, 910, 340, "Htqlhdk.Worker", "Identity và connection role riêng",
        [
            "Scheduler tạo scan_occurrence đến hạn; claim bằng lease/heartbeat",
            "ZapAutomationOrchestrator sinh plan, chạy/dừng và theo dõi ZAP",
            "ReportInspector kiểm nguồn, kích thước, host và SHA-256",
            "ArtifactStore lưu báo cáo; tạo import_batch Received để F-03 xử lý",
            "Idempotent, bounded retry; không dùng in-memory queue làm nguồn bền vững",
        ],
    )

    runner = architecture_component(
        c, 2280, 240, 440, 250, "Runner chuyên dụng", "Docker CLI / orchestrator",
        ["Quyền tối thiểu", "Giới hạn CPU/RAM", "Work directory riêng mỗi scan", "Không chứa API nghiệp vụ"],
        fill=WARN,
        accent="#FFEDD5",
    )
    zap = architecture_component(
        c, 2760, 240, 400, 310, "OWASP ZAP", "Container khóa image digest",
        ["Automation Framework", "Spider + passive scan", "Active scan khi được duyệt", "Sinh Traditional JSON/HTML", "Không kết nối PostgreSQL"],
        fill=WARN,
        accent="#FFEDD5",
    )
    target = architecture_component(
        c, 2520, 690, 480, 190, "Website mục tiêu", "Tài sản và phạm vi đã phê duyệt",
        ["Chỉ URL/context trong scope", "Quét trong khung giờ cho phép"],
        fill=SOFT,
    )

    db = architecture_component(
        c, 640, 1480, 430, 260, "PostgreSQL 17", "Nguồn sự thật nghiệp vụ",
        ["V001 baseline + V002 ZAP", "RBAC, audit, workflow", "scan_occurrence + zap_execution", "Runtime không có DDL/superuser"],
    )
    store_box = architecture_component(
        c, 1140, 1480, 430, 260, "Kho artifact riêng", "raw / evidence / exports",
        ["JSON/HTML và bằng chứng", "Private + mã hóa", "Checksum và metadata bất biến", "Local pilot → object storage production"],
    )
    idp = architecture_component(
        c, 1640, 1480, 350, 220, "IdP OIDC", "Danh tính ổn định",
        ["Claim tối thiểu", "Không tự cấp quyền tài sản"],
    )
    kms = architecture_component(
        c, 2060, 1480, 390, 240, "KMS / Secrets", "Workload identity",
        ["DB chỉ lưu key/secret reference", "Không ghi secret vào log/plan", "Ví dụ env:HTQLHDK_ZAP_*"],
    )
    telemetry = architecture_component(
        c, 2520, 1480, 620, 240, "Nền tảng quan sát", "OpenTelemetry",
        ["Log, metric và trace đã lọc", "Không nhận payload lỗ hổng", "Cảnh báo lease, timeout và lỗi runner"],
    )

    connect(c, users, "bottom", browser, "top", "sử dụng")
    connect(c, browser, "right", edge, "left", "HTTPS")
    connect(c, edge, "right", ui, "left", "static assets")
    connect(c, edge, "right", api, "left", "", route=[(1010, 540), (1580, 540), (1580, 475)])
    c.text(1300, 510, "REST / upload stream", 16, fill=MUTED, anchor="middle")
    connect(c, ui, "right", api, "left", "", route=[(1580, 325), (1580, 475)])
    c.text(1580, 350, "API / session", 15, fill=MUTED, anchor="middle")

    connect(c, api, "bottom", db, "top", "api_runtime + audit", route=[(1830, 1260), (855, 1260)])
    connect(c, api, "bottom", store_box, "top", "stream theo quyền", route=[(1900, 1300), (1355, 1300)])
    connect(c, api, "bottom", idp, "top", "OIDC", route=[(1960, 1340), (1815, 1340)])
    connect(c, api, "bottom", kms, "top", "secret reference", route=[(2020, 1380), (2255, 1380)])
    connect(c, api, "bottom", telemetry, "top", "telemetry đã lọc", route=[(2070, 1420), (2830, 1420)])

    connect(c, worker, "bottom", db, "top", "lease / trạng thái / import", route=[(1320, 1260), (900, 1260)])
    connect(c, worker, "bottom", store_box, "top", "lưu artifact", route=[(1510, 1340), (1400, 1340)])
    connect(c, worker, "bottom", kms, "top", "lấy secret", route=[(1840, 1300), (2200, 1300)])
    connect(c, worker, "bottom", telemetry, "top", "metric / cảnh báo", route=[(1980, 1260), (2920, 1260)])

    connect(c, worker, "right", runner, "left", "plan + chạy/dừng", route=[(2160, 960), (2160, 365)])
    connect(c, runner, "right", zap, "left", "container biệt lập")
    connect(c, zap, "bottom", target, "top", "spider / scan")
    connect(c, zap, "left", worker, "right", "JSON / HTML", route=[(2700, 600), (2160, 600), (2160, 1010)])

    c.rect(2250, 1040, 930, 250, fill=SOFT, outline="#CBD5E1", radius=18, width=3)
    c.text(2290, 1070, "Kiểm soát ranh giới ZAP", 23, bold=True, fill=ACCENT)
    c.multiline(2290, 1120, [
        "• API không điều khiển ZAP và không nhận ZAP API key.",
        "• Mỗi lần quét dùng container, snapshot cấu hình và thư mục riêng.",
        "• ZAP không truy cập PostgreSQL; Worker kiểm tra báo cáo trước khi tiếp nhận.",
        "• Không nhận YAML/script tùy ý; active scan mặc định tắt cho profile mới.",
    ], 19, gap=8)

    c.save("05_kien_truc_he_thong")


if __name__ == "__main__":
    build_erd()
    build_activity()
    build_state()
    build_dfd()
    build_architecture()
    print(f"Generated diagrams in {OUT}")
