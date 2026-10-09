from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root = Path(r"C:\Users\Admin\Documents\ChatGPT\HTQLHDK\_report_work")
im = Image.new("RGB", (1800, 790), "white")
d = ImageDraw.Draw(im)
font = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 34)
bold = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 34)
small = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 27)
ink = "#17212d"
line = "#425366"
fill = "#edf2f7"
boxes = {
    "asset": (40, 100, 350, 310, "Tài sản", "asset", ["PK id", "UK code"]),
    "finding": (485, 100, 795, 310, "Phát hiện", "finding", ["PK id", "FK asset_id"]),
    "occ": (930, 100, 1240, 310, "Lần xuất hiện", "finding_occurrence", ["FK finding_id", "FK scan_run_id"]),
    "run": (1375, 100, 1685, 310, "Lần quét", "scan_run", ["PK id", "FK plan_id"]),
    "ticket": (485, 480, 795, 690, "Phiếu khắc phục", "remediation_ticket", ["FK finding_id", "UK finding_id"]),
    "plan": (930, 480, 1240, 690, "Kế hoạch xác minh", "scan_plan", ["FK ticket_id", "Loại Verification"]),
}

def center(text, box, fnt, y):
    bb = d.textbbox((0, 0), text, font=fnt)
    x = (box[0] + box[2] - (bb[2] - bb[0])) / 2
    d.text((x, y), text, font=fnt, fill=ink)

for _, (x1, y1, x2, y2, title, table_name, details) in boxes.items():
    d.rounded_rectangle((x1, y1, x2, y2), radius=16, fill="white", outline=line, width=4)
    d.rounded_rectangle((x1, y1, x2, y1 + 101), radius=16, fill=fill, outline=line, width=4)
    d.rectangle((x1 + 2, y1 + 78, x2 - 2, y1 + 101), fill=fill)
    d.line((x1, y1 + 101, x2, y1 + 101), fill=line, width=3)
    center(title, (x1, y1, x2, y2), bold, y1 + 8)
    center(table_name, (x1, y1, x2, y2), small, y1 + 53)
    for i, detail in enumerate(details):
        center(detail, (x1, y1, x2, y2), font, y1 + 112 + 44 * i)

def edge(points, left_label, right_label, left_pos, right_pos):
    d.line(points, fill=line, width=5, joint="curve")
    d.text(left_pos, left_label, font=small, fill=ink)
    d.text(right_pos, right_label, font=small, fill=ink)

edge([(350, 205), (485, 205)], "1", "N", (365, 155), (445, 155))
edge([(795, 205), (930, 205)], "1", "N", (810, 155), (890, 155))
edge([(1240, 205), (1375, 205)], "N", "1", (1255, 155), (1330, 155))
edge([(640, 310), (640, 480)], "1", "0..1", (655, 315), (655, 427))
edge([(795, 585), (930, 585)], "1", "N", (810, 535), (890, 535))
edge([(1240, 585), (1530, 585), (1530, 310)], "1", "N", (1255, 535), (1545, 345))

d.text((45, 722), "FK: khóa ngoại   UK: khóa duy nhất   Quan hệ xác minh chỉ dùng kế hoạch loại Verification", font=font, fill=ink)
im.save(root / "erd_c_compact.png", optimize=True)
