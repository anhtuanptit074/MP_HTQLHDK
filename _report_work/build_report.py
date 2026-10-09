from pathlib import Path
from copy import deepcopy
from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.enum.style import WD_STYLE_TYPE
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT = Path(r"C:\Users\Admin\Documents\ChatGPT\HTQLHDK")
TEMPLATE = Path(r"C:\Users\Admin\Downloads\Mau Cuon Bao Cao Do An PT&TKHTTT.docx")
OUT = ROOT / "outputs" / "Bao_cao_HTQLHDK_Chuong_1_2_3.docx"
OUT.parent.mkdir(exist_ok=True)

doc = Document(TEMPLATE)
body = doc._element.body
for child in list(body):
    if child.tag != qn("w:sectPr"):
        body.remove(child)

sec = doc.sections[0]
sec.page_width, sec.page_height = Cm(21), Cm(29.7)
sec.top_margin = sec.bottom_margin = sec.left_margin = sec.right_margin = Cm(2.54)
sec.header_distance = sec.footer_distance = Cm(1.27)
sec.different_first_page_header_footer = True

styles = doc.styles
normal = styles["Normal"]
normal.font.name = "Times New Roman"
normal.font.size = Pt(13)
normal.font.color.rgb = RGBColor(0, 0, 0)
normal.paragraph_format.line_spacing = 1.3
normal.paragraph_format.space_after = Pt(6)
normal.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
for name, size, before, after in [("Heading 1", 16, 18, 8), ("Heading 2", 15, 12, 6)]:
    s = styles[name]
    s.font.name = "Times New Roman"
    s.font.size = Pt(size)
    s.font.bold = True
    s.font.color.rgb = RGBColor(0, 0, 0)
    s.paragraph_format.space_before = Pt(before)
    s.paragraph_format.space_after = Pt(after)
    s.paragraph_format.keep_with_next = True
    s.paragraph_format.line_spacing = 1.15
if "Caption" not in [s.name for s in styles]:
    cap = styles.add_style("Caption", WD_STYLE_TYPE.PARAGRAPH)
else:
    cap = styles["Caption"]
cap.font.name, cap.font.size = "Times New Roman", Pt(11)
cap.font.italic = True
cap.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
cap.paragraph_format.space_after = Pt(9)

def p(text="", *, bold=False, italic=False, align=None, style=None, first_indent=True):
    x = doc.add_paragraph(style=style)
    if first_indent and style is None:
        x.paragraph_format.first_line_indent = Cm(0.75)
    if align is not None:
        x.alignment = align
    r = x.add_run(text)
    r.bold, r.italic = bold, italic
    return x

def h1(text): return doc.add_heading(text, level=1)
def h2(text): return doc.add_heading(text, level=2)
def bullets(items):
    for item in items:
        x = doc.add_paragraph(style="List Paragraph")
        x.add_run("• " + item)
        x.paragraph_format.space_after = Pt(3)

def table(headers, rows, widths=None):
    t = doc.add_table(rows=1, cols=len(headers))
    t.alignment = WD_TABLE_ALIGNMENT.CENTER
    t.autofit = False
    tblPr = t._tbl.tblPr
    borders = OxmlElement("w:tblBorders")
    for edge in ("top", "left", "bottom", "right", "insideH", "insideV"):
        el = OxmlElement(f"w:{edge}")
        el.set(qn("w:val"), "single")
        el.set(qn("w:sz"), "4")
        el.set(qn("w:color"), "777777")
        borders.append(el)
    tblPr.append(borders)
    for i, label in enumerate(headers):
        t.rows[0].cells[i].text = label
    for row in rows:
        cells = t.add_row().cells
        for i, val in enumerate(row): cells[i].text = str(val)
    if widths:
        for row in t.rows:
            for i, width in enumerate(widths): row.cells[i].width = Cm(width)
    for i, row in enumerate(t.rows):
        for cell in row.cells:
            cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            for para in cell.paragraphs:
                para.paragraph_format.first_line_indent = Cm(0)
                para.paragraph_format.space_after = Pt(2)
                para.paragraph_format.line_spacing = 1.1
                for run in para.runs:
                    run.font.name, run.font.size = "Times New Roman", Pt(10.5)
                    if i == 0: run.bold = True
    trPr = t.rows[0]._tr.get_or_add_trPr()
    e = OxmlElement("w:tblHeader")
    e.set(qn("w:val"), "true")
    trPr.append(e)
    doc.add_paragraph().paragraph_format.space_after = Pt(0)
    return t

def fig(rel, caption, width_cm=15.5):
    path = ROOT / rel
    x = doc.add_paragraph()
    x.alignment = WD_ALIGN_PARAGRAPH.CENTER
    x.paragraph_format.keep_with_next = True
    x.add_run().add_picture(str(path), width=Cm(width_cm))
    p(caption, style="Caption", first_indent=False)

def page(): doc.add_page_break()

# Trang bìa: giữ cấu trúc chữ của mẫu, thay các ô thông tin chưa được cung cấp bằng chỗ điền.
for line in ["BỘ KHOA HỌC VÀ CÔNG NGHỆ", "HỌC VIỆN CÔNG NGHỆ BƯU CHÍNH VIỄN THÔNG", "-----------------------------"]:
    x = p(line, bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, first_indent=False)
    for r in x.runs: r.font.size = Pt(15)
for _ in range(4): p("", first_indent=False)
x = p("BÁO CÁO\nĐỒ ÁN MÔN HỌC", bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, first_indent=False)
for r in x.runs: r.font.size = Pt(28)
p("MÔN HỌC: PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG THÔNG TIN", bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, first_indent=False)
p("ĐỀ TÀI: HỆ THỐNG QUẢN LÝ QUÉT LỖ HỔNG WEBSITE ĐỊNH KỲ", bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, first_indent=False)
p("Báo cáo Chương I, II và III · Cập nhật ngày 28/09/2026", align=WD_ALIGN_PARAGRAPH.CENTER, first_indent=False)
for _ in range(4): p("", first_indent=False)
p("Giảng viên hướng dẫn: [Điền họ tên]", first_indent=False)
p("Nhóm thực hiện: [Điền tên nhóm và thành viên]", first_indent=False)
p("MSSV / lớp: [Điền thông tin]", first_indent=False)
for _ in range(3): p("", first_indent=False)
p("TP. Hồ Chí Minh, tháng 09/2026", align=WD_ALIGN_PARAGRAPH.CENTER, first_indent=False)
page()

h1("MỤC LỤC")
for title, subs in [
    ("CHƯƠNG I. TỔNG QUAN", ["1.1. Giới thiệu đề tài", "1.2. Mục tiêu và phạm vi", "1.3. Cơ sở lý thuyết và nền tảng kỹ thuật"]),
    ("CHƯƠNG II. PHÂN TÍCH NỘI DUNG, YÊU CẦU", ["2.1. Quy trình nghiệp vụ", "2.2. Tác nhân và yêu cầu chức năng", "2.3. Yêu cầu hệ thống và chất lượng"]),
    ("CHƯƠNG III. PHÂN TÍCH THIẾT KẾ HỆ THỐNG", ["3.1. Use case", "3.2. Trạng thái và tuần tự", "3.3. Hoạt động, lớp và dữ liệu", "3.4. Giao diện và xử lý"]),
]:
    p(title, bold=True, first_indent=False)
    for sub in subs: p("    " + sub, first_indent=False)
p("TÀI LIỆU THAM KHẢO", bold=True, first_indent=False)
h1("DANH MỤC TỪ VIẾT TẮT")
table(["Ký hiệu", "Diễn giải"], [
    ("ATTT", "An toàn thông tin"), ("HTQLHDK", "Hệ thống quản lý quét lỗ hổng website định kỳ"),
    ("OWASP ZAP", "OWASP Zed Attack Proxy"), ("API", "Giao diện lập trình ứng dụng"),
    ("ERD", "Sơ đồ thực thể và quan hệ"), ("BCNF", "Dạng chuẩn Boyce–Codd"),
    ("OIDC", "OpenID Connect"), ("SLA", "Cam kết mức dịch vụ"),
], [3.5, 12.3])
page()

h1("CHƯƠNG I. TỔNG QUAN")
h2("1.1. Giới thiệu đề tài")
p("Các đơn vị quản lý website cần phát hiện lỗ hổng theo chu kỳ, nhưng một kết quả quét chỉ có giá trị khi gắn được với tài sản, phạm vi được phép, người chịu trách nhiệm và quá trình khắc phục. HTQLHDK được xây dựng để quản lý liên tục chuỗi công việc này: khai báo tài sản, duyệt phạm vi, lập lịch, điều phối OWASP ZAP, tiếp nhận báo cáo, phân loại phát hiện và theo dõi xác minh [1], [2].")
p("Bản cập nhật mới nhất bổ sung phương án tự động điều phối OWASP ZAP bằng Worker riêng. Mỗi lịch tự động đến hạn có thể sinh một lần quét, kế hoạch Automation Framework, container ZAP biệt lập và lô nhập báo cáo. Song song, nhóm thiết kế đề xuất mô hình dữ liệu đích 13 bảng để tập trung bốn chức năng chính. Phần SQL hiện hành chưa chuyển sang mô hình này; báo cáo phân biệt rõ thiết kế đích và trạng thái triển khai [2], [3].")
h2("1.2. Mục tiêu của đề tài")
bullets([
    "Quản lý website và phiên bản phạm vi quét được chủ tài sản xác nhận, chuyên viên ATTT phê duyệt.",
    "Lập lịch định kỳ hoặc yêu cầu quét xác minh; chỉ thực hiện trong khung giờ và phạm vi đã duyệt.",
    "Điều phối OWASP ZAP an toàn; nhận báo cáo gốc, kiểm tính toàn vẹn và chuẩn hóa phát hiện.",
    "Phân biệt Risk/Confidence của ZAP với mức độ cuối cùng do ATTT xác nhận; chống trùng và nhận diện tái xuất hiện.",
    "Giao việc khắc phục, theo dõi hạn và bằng chứng, quét xác minh trước khi đóng hoặc mở lại phát hiện.",
    "Giới hạn truy cập báo cáo nhạy cảm theo vai trò và tài sản; ghi nhật ký hành động quan trọng."
])
h2("1.3. Phạm vi áp dụng")
p("Đối tượng áp dụng là website do đơn vị quản lý hoặc được cho phép kiểm thử. Hệ thống không tự suy diễn quyền quét từ việc biết URL: tài sản, phiên bản phạm vi, cấu hình ZAP và cửa sổ thời gian phải hợp lệ trước khi chạy. OWASP ZAP là công cụ quét bên ngoài duy nhất trong phạm vi đề tài. Lát cắt cốt lõi gồm F-01 quản lý tài sản, F-02 lập lịch/quét, F-03 tiếp nhận kết quả và F-04 khắc phục/xác minh [1], [2].")
p("Báo cáo này tập trung phân tích và thiết kế. Màn hình nghiệp vụ, parser F-03 đầy đủ, kiểm quyền API cuối cùng và triển khai production chưa được xem là đã hoàn tất chỉ vì đã có wireframe, OpenAPI hoặc Worker nền. Các chính sách như SLA, lưu giữ dữ liệu, hạn tải lên, IdP và kho tệp production cần đơn vị sử dụng phê duyệt [4], [5].")
h2("1.4. Cơ sở lý thuyết")
p("Quản lý lỗ hổng là vòng lặp phát hiện, đánh giá, xử lý và xác minh. Công cụ quét sinh Risk và Confidence theo chứng cứ kỹ thuật; hệ thống nghiệp vụ phải lưu chúng riêng với Final Severity vì mức độ cuối cùng còn phụ thuộc tác động lên tài sản và đánh giá của ATTT. Một kết quả không xuất hiện trong lần quét sau chưa đủ để tự động kết luận đã khắc phục nếu vùng bao phủ quét chưa được xác nhận [1], [2].")
p("Thiết kế dữ liệu áp dụng nguyên tắc chuẩn hóa: mỗi thực thể hoặc quan hệ nhiều giá trị có vòng đời riêng; dữ liệu có thể suy ra như số liệu tổng hợp, trạng thái hiện hành hoặc danh sách cần chú ý được truy vấn thay vì lưu lặp. Bản thiết kế đích 13 bảng được mô tả ở mức BCNF, nhưng cần migration và kiểm thử trước khi trở thành schema chạy thực tế [3].")
h2("1.5. Nền tảng kỹ thuật")
table(["Thành phần", "Công nghệ / vai trò"], [
    ("Giao diện", "React 19.2, TypeScript, Vite; hiện có nền ứng dụng và hồ sơ wireframe."),
    ("API", "ASP.NET Core/.NET 10, REST JSON; OpenAPI 3.1 là hợp đồng thiết kế."),
    ("Worker", ".NET 10, Npgsql; điều phối lịch và phiên ZAP khi được bật."),
    ("Dữ liệu", "PostgreSQL 17; V001 có 27 bảng, V002 bổ sung 3 bảng ZAP."),
    ("Máy quét", "OWASP ZAP 2.17.0 qua container và Automation Framework plan."),
], [3.5, 12.3])
fig("architecture/rendered/02_container_components.png", "Hình 1.1. Sơ đồ thành phần nền tảng hệ thống (nguồn: hồ sơ kiến trúc [4]).", 15.6)

page()
h1("CHƯƠNG II. PHÂN TÍCH NỘI DUNG, YÊU CẦU")
h2("2.1. Đối tượng sử dụng và ranh giới trách nhiệm")
table(["Đối tượng", "Trách nhiệm chính"], [
    ("Chủ tài sản (OWNER)", "Khai báo tài sản, xác nhận phạm vi, cung cấp tác động nghiệp vụ, theo dõi kết quả thuộc tài sản."),
    ("Chuyên viên ATTT (SECURITY)", "Duyệt phạm vi, rà soát kết quả, xác nhận mức độ, quyết định xác minh và truy cập báo cáo gốc theo quyền riêng."),
    ("Người vận hành quét (SCAN_OPERATOR)", "Lập/giám sát lịch và lần quét; xử lý lỗi kỹ thuật, không tự phê duyệt phạm vi."),
    ("Người khắc phục (REMEDIATOR)", "Nhận phiếu, cập nhật biện pháp, tiến độ và bằng chứng; yêu cầu quét xác minh."),
    ("Quản trị/Auditor", "Quản lý quyền hoặc xem nhật ký trong phạm vi được giao; quyền quản trị không mặc nhiên mở báo cáo gốc."),
    ("OWASP ZAP/Worker", "Tác nhân kỹ thuật quét ngoài hệ thống; Worker điều phối, kiểm báo cáo và ghi trạng thái."),
], [4.3, 11.5])
h2("2.2. Quy trình 1: quản lý tài sản và phê duyệt phạm vi")
p("Chủ tài sản tạo bản ghi website ở trạng thái nháp, khai báo URL gốc, mức quan trọng, môi trường và người phụ trách. Phạm vi được lưu theo phiên bản, gồm các quy tắc include/exclude và khung giờ quét. Chủ tài sản xác nhận nội dung trước khi ATTT phê duyệt. Phiên bản đã duyệt phải bất biến; sửa đổi tạo phiên bản mới. Tài sản tạm dừng hoặc ngừng hoạt động không được lập lịch tự động mới [1], [3].")
h2("2.3. Quy trình 2: lập lịch và điều phối quét ZAP")
p("Người vận hành gắn lịch với tài sản, phiên bản phạm vi và ZAP profile đã được phê duyệt. Khi lịch tự động đến hạn, Worker tạo scan occurrence và zap execution, kết xuất plan YAML từ dữ liệu được kiểm soát rồi khởi chạy một container ZAP biệt lập. Plan có context, spider, thời gian chờ passive scan, active scan khi được bật và báo cáo Traditional JSON/HTML. Worker không nhận YAML tùy ý từ người dùng [2].")
p("Điều kiện dispatch đồng thời gồm lịch Active, tài sản Active, scope Approved và profile Active. Active scan mặc định tắt. Worker có lease/heartbeat để phát hiện lần chạy bị bỏ dở; khi hết hạn, nó đánh dấu lỗi để rà soát thay vì tự quét lại một mục tiêu đã có thể nhận request. Lịch thủ công vẫn có thể tiếp tục theo quy trình cũ [2].")
h2("2.4. Quy trình 3: nhận kết quả, đánh giá và khắc phục")
p("Sau khi ZAP hoàn thành, Worker kiểm kích thước và định dạng báo cáo, xác nhận nguồn ZAP, đối chiếu host đích, tính SHA-256 và lưu artifact. Giao dịch nhận kết quả cập nhật lần quét và tạo import_batch ở trạng thái Received. Parser F-03 là bước tiếp theo để chuẩn hóa, chống trùng và ghi finding; Worker không tự đánh giá Final Severity hay đóng finding [2].")
p("ATTT rà soát từng phát hiện, phân biệt báo sai hoặc ngoài phạm vi, xác nhận mức độ cuối cùng và giao phiếu khắc phục. Người được giao cập nhật biện pháp, tiến độ và bằng chứng. Sau khi yêu cầu xác minh, kết quả quét mới được đối chiếu với fingerprint và vùng bao phủ để quyết định đóng hoặc mở lại. Quy trình giữ lịch sử và dấu vết quyết định [1].")
fig("so_do_cap_nhat/rendered/02_activity_quy_trinh_tong_the.png", "Hình 2.1. Sơ đồ hoạt động tổng thể sau khi bổ sung điều phối ZAP (nguồn: [2]).", 13.0)
h2("2.5. Yêu cầu chức năng nghiệp vụ")
table(["Mã", "Yêu cầu và kết quả cần đạt"], [
    ("F-01", "Tạo, cập nhật, xác nhận và phê duyệt tài sản/phạm vi; lưu phiên bản và chặn quét sai phạm vi."),
    ("F-02", "Tạo lịch/phiên quét; điều phối ZAP theo profile, theo dõi trạng thái, timeout và lỗi."),
    ("F-03", "Nhận báo cáo nguồn, kiểm checksum/định dạng/phạm vi, chuẩn hóa, chống trùng, lưu finding."),
    ("F-04", "Rà soát mức độ, giao phiếu, theo dõi hạn, xác minh và đóng/mở lại phát hiện."),
    ("Hỗ trợ", "Kiểm quyền theo vai trò và tài sản; masking dữ liệu; audit hành động nhạy cảm; truy xuất báo cáo theo quyền."),
], [2.1, 13.7])
h2("2.6. Quy tắc nghiệp vụ và chất lượng")
bullets([
    "Mọi lần quét phải dựa trên phạm vi và thời gian được duyệt; URL ngoài phạm vi bị chặn hoặc tách xử lý theo quy tắc.",
    "Báo cáo nguồn và metadata checksum được giữ bất biến; không ghi token, Attack/Evidence hay nội dung thô vào log.",
    "Quyền đọc raw report tách khỏi quyền quản trị thông thường; API kiểm lại quyền ở mọi request, không dựa vào việc UI ẩn nút.",
    "Các thao tác ghi có guard trạng thái, kiểm soát lặp và audit; tác vụ nền phục hồi được sau lỗi nhưng không tự lặp quét đã phát request.",
    "Giao diện phải hỗ trợ trạng thái chờ, lỗi, trùng, không có dữ liệu và bị từ chối; thời gian hiển thị kèm múi giờ.",
    "Kiểm thử phải bao gồm đường thành công và từ chối: sai quyền, sai tài sản, phạm vi hết hiệu lực, báo cáo không hợp lệ, rollback và chạy đồng thời."
])

page()
h1("CHƯƠNG III. PHÂN TÍCH THIẾT KẾ HỆ THỐNG")
h2("3.1. Sơ đồ use case")
p("Các use case được nhóm theo bốn luồng F-01 đến F-04 và các kiểm soát hỗ trợ. Chủ tài sản khởi tạo/xác nhận phạm vi; ATTT duyệt và đánh giá; người vận hành quản lý quét; người khắc phục xử lý phiếu. Worker/ZAP là tác nhân kỹ thuật, không thay vai trò ra quyết định của con người. Hồ sơ baseline có 63 use case và 16 sơ đồ hoạt động; sơ đồ tổng thể dưới đây thể hiện quan hệ chính [1], [6].")
fig("visual_paradigm_uml_md/rendered/01_use_case_tong_the.png", "Hình 3.1. Sơ đồ use case tổng thể của HTQLHDK (nguồn: [6]).", 7.4)
h2("3.2. Sơ đồ trạng thái")
p("Vòng đời kỹ thuật zap_execution bắt đầu từ Queued, đi qua Preparing Plan, Starting Runner, Running, Collecting Report đến Report Received. Các nhánh kết thúc lỗi gồm Failed, Timed Out và Cancelled. Trạng thái nghiệp vụ scan_occurrence vẫn được lưu riêng để không phá hợp đồng hiện có; một báo cáo hợp lệ mới cho phép chuyển lần quét thành Completed và tạo lô Received [2].")
fig("so_do_cap_nhat/rendered/03_state_zap_execution.png", "Hình 3.2. Sơ đồ trạng thái phiên thực thi ZAP (nguồn: [2]).", 15.5)
h2("3.3. Sơ đồ tuần tự và xử lý chính")
p("Trình tự xử lý tự động gồm: bộ lập lịch kiểm điều kiện, repository tạo occurrence/execution, Worker nhận lease, sinh plan theo scope/profile, runner khởi chạy ZAP, bộ kiểm báo cáo xác minh định dạng và checksum, kho artifact lưu tệp, giao dịch cơ sở dữ liệu cập nhật trạng thái và tạo import_batch. Luồng F-03 xử lý lô sau đó; lỗi ở bất kỳ bước nào cần lưu trạng thái và thông tin lỗi đã che [2].")
table(["Bước", "Thành phần", "Kiểm soát quan trọng"], [
    ("1", "Scheduler/Repository", "Lịch, tài sản, scope và profile còn hiệu lực; chống dispatch trùng."),
    ("2", "Worker/Plan Renderer", "Lấy snapshot cấu hình; không nhận YAML tùy ý; secret chỉ là tham chiếu."),
    ("3", "Docker ZAP Runner", "Container riêng, giới hạn tài nguyên, chỉ quét mục tiêu được duyệt."),
    ("4", "Report Inspector/Artifact Store", "Kiểm JSON, host, kích thước, checksum; lưu báo cáo bất biến."),
    ("5", "Repository/F-03", "Cập nhật scan và tạo import_batch Received cùng giao dịch; parser xử lý tiếp."),
], [1.5, 4.2, 10.1])
h2("3.4. Sơ đồ hoạt động")
p("Sơ đồ hoạt động tổng thể ở Hình 2.1 mô tả các nhánh quyết định từ xác nhận tài sản đến xử lý phát hiện. Ở nhánh tự động, một lịch đến hạn chỉ chuyển sang thực thi khi đủ điều kiện an toàn; nhánh lỗi giữ nguyên hồ sơ kỹ thuật và chuyển cho người vận hành xử lý. Ở nhánh khắc phục, cập nhật của người được giao chưa làm phát hiện đóng ngay: phải có yêu cầu và kết quả xác minh độc lập [1], [2].")
h2("3.5. Sơ đồ lớp miền nghiệp vụ")
p("Mô hình lớp miền được suy ra từ thiết kế đích 13 bảng. Asset sở hữu các ScopeRevision; mỗi ScopeRevision có ScopeRule và ScanWindow. ScanPlan liên kết ScopeRevision với ZapScanProfile và sinh ScanRun. ScanRun ghi sự xuất hiện của Finding; Finding có thể gắn RemediationTicket. Các hành động quan trọng phát sinh AuditLog. Đây là mô hình lớp phân tích, chưa phải các lớp mã nguồn đã triển khai [3].")
table(["Lớp", "Thuộc tính/hành vi chính", "Quan hệ"], [
    ("Asset", "Mã, URL gốc, trạng thái; yêu cầu kích hoạt/tạm dừng", "1–N ScopeRevision"),
    ("ScopeRevision", "Số phiên bản, hiệu lực, trạng thái duyệt", "1–N ScopeRule/ScanWindow"),
    ("ScanPlan / ScanRun", "Lịch hoặc yêu cầu xác minh; một lần chạy và trạng thái", "ScanPlan 1–N ScanRun"),
    ("Finding", "Fingerprint, Risk nguồn, Final Severity, trạng thái", "N–N ScanRun qua FindingOccurrence"),
    ("RemediationTicket", "Người phụ trách, hạn, tiến độ, bằng chứng", "N–1 Finding"),
], [3.2, 6.7, 5.9])
h2("3.6. Thiết kế cơ sở dữ liệu")
p("Thiết kế cập nhật mới nhất đề xuất 13 bảng vật lý theo BCNF cho bốn luồng cốt lõi. Các nhóm bảng gồm định danh/quyền; tài sản/phạm vi; kế hoạch/phiên ZAP; phát hiện; khắc phục; audit. Dữ liệu hiện hành như số lượng phát hiện, báo cáo tổng hợp và thông báo công việc được truy vấn thay vì lưu lặp. Đây là mô hình đích, chưa phải schema triển khai [3].")
table(["Nhóm", "Các bảng trong mô hình đích 13 bảng"], [
    ("Định danh", "app_user, user_role"),
    ("Tài sản/phạm vi", "asset, asset_scope_revision, asset_scope_rule, asset_scan_window"),
    ("Quét ZAP", "zap_scan_profile, scan_plan, scan_run"),
    ("Kết quả", "finding, finding_occurrence"),
    ("Khắc phục/audit", "remediation_ticket, audit_log"),
], [4.0, 11.8])
p("Quan hệ chính là asset → asset_scope_revision → asset_scope_rule/asset_scan_window; scope và zap_scan_profile → scan_plan → scan_run; scan_run → finding_occurrence ← finding; finding → remediation_ticket; mọi hành động nhạy cảm → audit_log. Phiên bản phạm vi và profile được khóa sau phê duyệt; kết quả nguồn và dấu vết kiểm toán cần bảo vệ tính bất biến [3].")
p("Ở mã nguồn hiện hành, V001 tạo 27 bảng; V002 thêm zap_scan_profile, zap_execution và zap_artifact, đồng thời mở rộng scan_schedule. Vì vậy SQL hiện tại tương đương nền 27 bảng cộng 3 bảng tự động hóa, không thể mô tả là đã dùng 13 bảng. Chuyển sang mô hình đích cần migration, ánh xạ dữ liệu, cập nhật API/Worker và kiểm thử hồi quy riêng [2], [3].", bold=False)
h2("3.7. Thiết kế giao diện")
p("Hồ sơ giao diện thiết kế 32 màn hình và ánh xạ với 57 operation API. Điều hướng chính theo công việc: Tài sản, Lịch & lần quét, Lô nhập, Phát hiện, Phiếu khắc phục, Xác minh và Báo cáo. Màn hình chi tiết phát hiện hiển thị tách biệt Source Risk, Confidence và Final Severity; hành động hợp lệ thay đổi theo vai trò, trạng thái và phạm vi tài sản [5].")
p("Các màn hình ưu tiên để hiện thực gồm chi tiết tài sản/phạm vi, lịch quét, trạng thái phiên ZAP, lô nhập, worklist finding, chi tiết phiếu và xác minh. Báo cáo gốc/evidence nhạy cảm phải qua endpoint và bước xác nhận riêng; UI không render HTML từ ZAP. Wireframe hiện là thiết kế, chưa phải bằng chứng đã hoàn thiện frontend nghiệp vụ [5].")
h2("3.8. Thiết kế xử lý và bảo mật")
fig("so_do_cap_nhat/rendered/04_dfd_level_1.png", "Hình 3.3. Sơ đồ luồng dữ liệu mức 1 của hệ thống (nguồn: [2]).", 15.4)
p("Hệ thống dùng API/Worker cùng release nhưng tách workload. API phục vụ giao diện, kiểm quyền theo quan hệ nghiệp vụ và ghi audit. Worker giữ quyền điều phối ZAP, không đặt Docker socket hay ZAP API key trong API. Production cần runner quyền tối thiểu, kho artifact riêng tư có mã hóa và cơ chế quản lý secret phù hợp. Các lựa chọn hạ tầng cụ thể còn chờ phê duyệt [2], [4].")
h2("3.9. Kiểm chứng thiết kế và trạng thái hiện tại")
table(["Hạng mục", "Bằng chứng/giới hạn hiện có"], [
    ("Thiết kế", "Có yêu cầu nghiệp vụ, use case, activity, OpenAPI 57 operation, 32 wireframe và hồ sơ kiến trúc."),
    ("Dữ liệu", "Bộ SQL nền 27 bảng có 75/75 kiểm tra artifact đạt trên PostgreSQL nhúng; mô hình 13 bảng chưa migration."),
    ("Ứng dụng", "Có nền React/.NET và Worker điều phối ZAP; parser F-03 và màn hình nghiệp vụ đầy đủ còn là công việc tiếp theo."),
    ("Vận hành", "ZAP automation mặc định tắt; cần phê duyệt mục tiêu, cấu hình secret/runner và thử trên staging trước khi dùng thật."),
], [3.2, 12.6])
p("Các kết quả kiểm tra ở trên xác nhận từng phần tài liệu và nền kỹ thuật, chưa chứng minh hệ thống end-to-end đã nghiệm thu. Đặc biệt, kiểm tra SQL 75/75 không bao phủ phân quyền API, parser ZAP thực, bảo vệ kho tệp hay giao dịch đồng thời. Đây là các cổng cần hoàn tất ở giai đoạn triển khai/kiểm thử tiếp theo [2], [7].")

h1("TÀI LIỆU THAM KHẢO")
refs = [
    "[1] HTQLHDK, doi_tuong_sd_quy_trinh_hd_tich_hop_zap_tu_dong.md, hồ sơ đối tượng và quy trình cập nhật.",
    "[2] HTQLHDK, docs/ZAP_AUTOMATION_FRAMEWORK_CHANGES.md; so_do_cap_nhat/README.md, phần bổ sung tự động hóa ZAP.",
    "[3] HTQLHDK, database/README.md; database/08_thiet_ke_3nf_tinh_gon.md; database/09_giai_thich_chi_tiet_13_bang.md.",
    "[4] HTQLHDK, architecture/README.md và architecture/01_kien_truc_logic_va_trien_khai.md.",
    "[5] HTQLHDK, ui_design/README.md; ui_design/02_danh_muc_man_hinh_va_wireframe.md.",
    "[6] HTQLHDK, tai_lieu_use_case/README.md; visual_paradigm_uml_md/README.md.",
    "[7] HTQLHDK, docs/development/08_verification_report.md; database/validation/database_test_results.json.",
]
for ref in refs: p(ref, first_indent=False)

header = sec.header.paragraphs[0]
header.text = "Báo cáo đồ án · HTQLHDK"
header.alignment = WD_ALIGN_PARAGRAPH.RIGHT
for r in header.runs: r.font.name, r.font.size = "Times New Roman", Pt(9)
for hp in sec.first_page_header.paragraphs: hp.text = ""
for fp in sec.first_page_footer.paragraphs: fp.text = ""
footer = sec.footer.paragraphs[0]
footer.text = ""
footer.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = footer.add_run()
fld = OxmlElement("w:fldSimple")
fld.set(qn("w:instr"), "PAGE")
run._r.addnext(fld)

settings = doc.settings._element
update = OxmlElement("w:updateFields")
update.set(qn("w:val"), "true")
settings.append(update)

doc.save(OUT)
print(OUT)
