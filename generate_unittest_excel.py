"""
generate_unittest_excel.py
--------------------------
Generate UnitestKCPM.xlsx theo đúng format UnitestCuaBao của thầy.

Format chuẩn (phân tích từ UnitestCuaBao.xlsx):
- Sheet1: Tổng hợp tất cả functions
- Function sheets: Mỗi sheet = 1 chức năng, dùng condition matrix
  * Header: Function Code, Function Name, Created By, Lines of Code, Test Requirement
  * Summary: Passed/Failed/Untested/N/A/B, Total Test Cases
  * UTCID row: mỗi cột là 1 test case ID
  * Condition matrix: mỗi hàng = 1 điều kiện/input, đánh O
  * Result section: Return (HTTP status), Exception, Log message
  * Execution: Type (N/A/B), Passed/Failed, Executed Date, Defect ID

Chạy: python generate_unittest_excel.py
Output: UnitestKCPM.xlsx (đặt ở root repo)
"""

import sys
sys.stdout.reconfigure(encoding='utf-8')

from openpyxl import Workbook
from openpyxl.styles import (
    PatternFill, Font, Alignment, Border, Side, GradientFill
)
from openpyxl.utils import get_column_letter
from datetime import date
import os

# ──────────────────────────────────────────────────────────────────────────────
# STYLE CONSTANTS (matching UnitestCuaBao.xlsx look)
# ──────────────────────────────────────────────────────────────────────────────
COLOR_HEADER_BG   = "4472C4"   # Blue header
COLOR_HEADER_FG   = "FFFFFF"   # White text
COLOR_SECTION_BG  = "D9E1F2"   # Light blue section
COLOR_MARK_BG     = "E2EFDA"   # Light green for "O" marks
COLOR_PASS_BG     = "C6EFCE"   # Green for Passed
COLOR_FAIL_BG     = "FFC7CE"   # Red for Failed
COLOR_NA_BG       = "FFEB9C"   # Yellow for N/A
COLOR_COND_LABEL  = "BDD7EE"   # Condition label background
COLOR_RESULT_BG   = "FFF2CC"   # Result section background
COLOR_NORMAL_BG   = "FFFFFF"

THIN = Side(style='thin', color='000000')
BORDER = Border(left=THIN, right=THIN, top=THIN, bottom=THIN)

def hdr_fill(hex_color):
    return PatternFill("solid", fgColor=hex_color)

def bold_font(size=10, color="000000", italic=False):
    return Font(name="Calibri", bold=True, size=size, color=color, italic=italic)

def normal_font(size=10, color="000000"):
    return Font(name="Calibri", bold=False, size=size, color=color)

def centered(wrap=False):
    return Alignment(horizontal="center", vertical="center", wrap_text=wrap)

def left_aligned(wrap=True):
    return Alignment(horizontal="left", vertical="center", wrap_text=wrap)

def apply_border(ws, min_row, max_row, min_col, max_col):
    for row in ws.iter_rows(min_row=min_row, max_row=max_row,
                             min_col=min_col, max_col=max_col):
        for cell in row:
            cell.border = BORDER

def set_cell(ws, row, col, value, fill=None, font=None, align=None, border=True):
    cell = ws.cell(row=row, column=col, value=value)
    if fill:  cell.fill  = fill
    if font:  cell.font  = font
    if align: cell.alignment = align
    if border: cell.border = BORDER
    return cell

# ──────────────────────────────────────────────────────────────────────────────
# DATA DEFINITIONS
# ──────────────────────────────────────────────────────────────────────────────

# Each function dict:
# {
#   code, name, created_by, executed_by, lines_of_code, test_requirement,
#   jira_key,
#   test_cases: [{ id, description, conditions: [{label, value}], returns, type, passed, date, defect, note }]
# }

FUNCTIONS = [
    {
        "code": "F01",
        "name": "Chức năng Đăng ký tài khoản",
        "created_by": "Tạ Đức Bảo",
        "executed_by": "Tạ Đức Bảo",
        "lines_of_code": "120",
        "test_requirement": "Citizen/Enterprise đăng ký tài khoản mới; email unique; password >= 8 ký tự; role = citizen hoặc enterprise",
        "jira_key": "KIEM-4",
        "test_cases": [
            {
                "id": "AUTH-01",
                "description": "[Happy Path] Đăng ký Citizen hợp lệ – nhận JWT token",
                "conditions": [
                    ("email",        "newcitizen@example.com (unique)"),
                    ("password",     "StrongPassword123! (>= 8 ký tự)"),
                    ("fullName",     "New Citizen (không rỗng)"),
                    ("role",         "Citizen"),
                ],
                "returns": "200 OK + JWT token",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-10",
                "defect": "",
                "note": "Đăng ký thành công, user được lưu DB"
            },
            {
                "id": "AUTH-02",
                "description": "[Negative] Đăng ký với email đã tồn tại – từ chối với 409",
                "conditions": [
                    ("email",        "existing@example.com (đã có trong DB)"),
                    ("password",     "Password123!"),
                    ("fullName",     "Another Citizen"),
                    ("role",         "Citizen"),
                ],
                "returns": "409 Conflict + message 'đã được sử dụng'",
                "http_code": 409,
                "type": "A",
                "passed": True,
                "date": "2026-04-10",
                "defect": "",
                "note": "Error guessing – duplicate email"
            },
            {
                "id": "AUTH-03",
                "description": "[Happy Path] Đăng nhập hợp lệ – nhận JWT token",
                "conditions": [
                    ("email",        "valid@example.com (active, tồn tại)"),
                    ("password",     "MySecretPassword123! (đúng)"),
                    ("account",      "Enterprise – active"),
                ],
                "returns": "200 OK + JWT token + Enterprise profile tạo auto",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-10",
                "defect": "",
                "note": "Auto-create Enterprise profile khi login lần đầu"
            },
            {
                "id": "AUTH-04",
                "description": "[Negative] Đăng nhập sai mật khẩu – từ chối với 401",
                "conditions": [
                    ("email",        "user@example.com (tồn tại)"),
                    ("password",     "WrongPassword! (SAI)"),
                    ("account",      "Citizen – active"),
                ],
                "returns": "401 Unauthorized + message 'không đúng'",
                "http_code": 401,
                "type": "A",
                "passed": True,
                "date": "2026-04-10",
                "defect": "",
                "note": "Error guessing – sai mật khẩu"
            },
            {
                "id": "AUTH-05",
                "description": "[Unit] GET /me trả về claims của user đã xác thực",
                "conditions": [
                    ("userId",       "GUID hợp lệ từ Claims"),
                    ("email",        "me@example.com"),
                    ("role",         "Admin"),
                    ("fullName",     "Admin User"),
                ],
                "returns": "200 OK + userId, email, role, fullName",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-10",
                "defect": "",
                "note": "Kiểm tra JWT claims parsing"
            },
        ]
    },
    {
        "code": "F02",
        "name": "Chức năng Tạo báo cáo rác (Citizen)",
        "created_by": "Nguyễn Minh Phụng",
        "executed_by": "Nguyễn Minh Phụng",
        "lines_of_code": "150",
        "test_requirement": "Citizen tạo waste report; phải có ít nhất 1 ảnh; WasteCategoryId hợp lệ; tọa độ trong phạm vi [-90,90] x [-180,180]; status mặc định = Pending",
        "jira_key": "KIEM-5",
        "test_cases": [
            {
                "id": "REP-01",
                "description": "[Happy Path] Tạo report với data đầy đủ hợp lệ",
                "conditions": [
                    ("WasteCategoryId", "1 (tồn tại trong DB)"),
                    ("Latitude",        "10.7769 (trong phạm vi [-90,90])"),
                    ("Longitude",       "106.7009 (trong phạm vi [-180,180])"),
                    ("Images",          "1 file JPG, size <= 5MB"),
                    ("Address",         "123 Nguyễn Trãi, Q.1"),
                ],
                "returns": "Guid report ID + status = Pending",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-12",
                "defect": "",
                "note": "Happy path – lưu report + upload ảnh"
            },
            {
                "id": "REP-02",
                "description": "[Negative] Tạo report không có ảnh – ném ArgumentException",
                "conditions": [
                    ("WasteCategoryId", "1"),
                    ("Latitude",        "10.7769"),
                    ("Longitude",       "106.7009"),
                    ("Images",          "null (KHÔNG CÓ ảnh)"),
                ],
                "returns": "ArgumentException 'At least one image is required'",
                "http_code": 400,
                "type": "A",
                "passed": True,
                "date": "2026-04-12",
                "defect": "",
                "note": "Error guessing – thiếu ảnh"
            },
            {
                "id": "REP-03",
                "description": "[Negative] WasteCategoryId không tồn tại – ném ArgumentException",
                "conditions": [
                    ("WasteCategoryId", "999 (KHÔNG tồn tại)"),
                    ("Latitude",        "10.7769"),
                    ("Longitude",       "106.7009"),
                    ("Images",          "null"),
                ],
                "returns": "ArgumentException 'Invalid waste category'",
                "http_code": 400,
                "type": "A",
                "passed": True,
                "date": "2026-04-12",
                "defect": "",
                "note": "Equivalence partitioning – invalid category"
            },
            {
                "id": "REP-04",
                "description": "[BVA] Tọa độ vượt boundary – ném ArgumentException",
                "conditions": [
                    ("Latitude",   "-91 (min- = dưới -90) / 91 (max+)"),
                    ("Longitude",  "-181 (min-) / 181 (max+)"),
                    ("Images",     "1 file JPG"),
                    ("Category",   "valid"),
                ],
                "returns": "ArgumentException 'Invalid latitude or longitude coordinates'",
                "http_code": 400,
                "type": "B",
                "passed": True,
                "date": "2026-04-12",
                "defect": "",
                "note": "BVA – 4 dirty test cases (Lat<-90, Lat>90, Lon<-180, Lon>180)"
            },
            {
                "id": "REP-05",
                "description": "[BVA] Tọa độ tại boundary max (90, 180) – tạo thành công",
                "conditions": [
                    ("Latitude",   "90 (max hợp lệ)"),
                    ("Longitude",  "180 (max hợp lệ)"),
                    ("Images",     "1 file JPG"),
                    ("Category",   "valid"),
                ],
                "returns": "Guid report ID + status = Pending",
                "http_code": 200,
                "type": "B",
                "passed": True,
                "date": "2026-04-12",
                "defect": "",
                "note": "BVA – max boundary hợp lệ"
            },
        ]
    },
    {
        "code": "F03",
        "name": "Chức năng Giao nhiệm vụ cho Collector (Enterprise)",
        "created_by": "Trần Quang Hoàng",
        "executed_by": "Trần Quang Hoàng",
        "lines_of_code": "100",
        "test_requirement": "Enterprise assign collector vào task; collector phải thuộc enterprise; task phải tồn tại",
        "jira_key": "KIEM-16",
        "test_cases": [
            {
                "id": "TASK-01",
                "description": "[Happy Path] Assign collector hợp lệ vào task",
                "conditions": [
                    ("taskId",      "GUID tồn tại"),
                    ("collectorId", "GUID collector thuộc enterprise"),
                    ("role",        "Enterprise"),
                ],
                "returns": "200 OK – task.CollectorId được cập nhật",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "Decision table – happy path"
            },
            {
                "id": "TASK-02",
                "description": "[Negative] Assign collector không thuộc enterprise – từ chối",
                "conditions": [
                    ("taskId",      "GUID tồn tại"),
                    ("collectorId", "GUID collector KHÔNG thuộc enterprise"),
                    ("role",        "Enterprise"),
                ],
                "returns": "403 Forbidden hoặc ArgumentException",
                "http_code": 403,
                "type": "A",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "Role-based access control"
            },
            {
                "id": "TASK-03",
                "description": "[Negative] Collector không tồn tại – 404",
                "conditions": [
                    ("taskId",      "GUID tồn tại"),
                    ("collectorId", "GUID không tồn tại trong DB"),
                ],
                "returns": "ArgumentException 'Collector not found'",
                "http_code": 404,
                "type": "A",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "Error guessing – nonexistent collector"
            },
        ]
    },
    {
        "code": "F04",
        "name": "Chức năng Chuyển trạng thái Task (Collector)",
        "created_by": "Nguyễn Thanh Duy",
        "executed_by": "Nguyễn Thanh Duy",
        "lines_of_code": "80",
        "test_requirement": "CollectionTask chuyển state: Assigned → OnTheWay → Collected; không cho phép chuyển ngược; SetOnTheWay khi không phải Assigned thì throw",
        "jira_key": "KIEM-14",
        "test_cases": [
            {
                "id": "TASK-07",
                "description": "[Happy Path] Tạo CollectionTask với status = Assigned",
                "conditions": [
                    ("reportId",    "GUID hợp lệ"),
                    ("enterpriseId","GUID hợp lệ"),
                ],
                "returns": "Status = Assigned, CollectorId = null, StatusLogs = rỗng",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "State Transition – initial state"
            },
            {
                "id": "TASK-08",
                "description": "[State Transition] Assigned → OnTheWay",
                "conditions": [
                    ("task.Status", "Assigned (tiền điều kiện)"),
                    ("action",      "SetOnTheWay()"),
                ],
                "returns": "Status = OnTheWay, StatusLogs có 1 entry",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "State transition diagram – hợp lệ"
            },
            {
                "id": "TASK-09",
                "description": "[Negative] SetOnTheWay khi task không phải Assigned – throw",
                "conditions": [
                    ("task.Status", "OnTheWay (đã chuyển rồi)"),
                    ("action",      "SetOnTheWay() lần 2"),
                ],
                "returns": "InvalidOperationException 'Task must be Assigned before going OnTheWay'",
                "http_code": 400,
                "type": "A",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "State transition – chuyển sai thứ tự"
            },
            {
                "id": "TASK-10",
                "description": "[State Transition] OnTheWay → Collected (Complete)",
                "conditions": [
                    ("task.Status", "OnTheWay (tiền điều kiện)"),
                    ("weight",      "12.5 kg"),
                    ("notes",       "'Collected at front gate'"),
                ],
                "returns": "Status = Collected, CompletedAt != null, StatusLogs = 2 entries",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "Complete – happy path"
            },
            {
                "id": "TASK-11",
                "description": "[Negative] Complete khi task chưa OnTheWay – throw",
                "conditions": [
                    ("task.Status", "Assigned (chưa OnTheWay)"),
                    ("action",      "Complete(10m, 'notes')"),
                ],
                "returns": "InvalidOperationException 'Task must be OnTheWay before Collected'",
                "http_code": 400,
                "type": "A",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "State transition – bỏ qua bước OnTheWay"
            },
            {
                "id": "TASK-12",
                "description": "[Unit] AssignCollector lưu collectorId vào task",
                "conditions": [
                    ("collectorId", "GUID hợp lệ"),
                    ("task.Status", "Assigned"),
                ],
                "returns": "task.CollectorId == collectorId",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-04-15",
                "defect": "",
                "note": "Unit test domain method"
            },
        ]
    },
    {
        "code": "F05",
        "name": "Chức năng E2E – Citizen tạo báo cáo",
        "created_by": "Trần Quang Hoàng",
        "executed_by": "Trần Quang Hoàng",
        "lines_of_code": "N/A",
        "test_requirement": "Citizen đăng ký, đăng nhập, điều hướng đến /citizen/create-report, gửi form báo cáo hợp lệ; hệ thống lưu report và redirect về /citizen/reports",
        "jira_key": "KIEM-FE",
        "test_cases": [
            {
                "id": "E2E-002A",
                "description": "[E2E – Happy Path] Citizen đăng ký mới và vào dashboard thành công",
                "conditions": [
                    ("name",        "E2E Test Citizen"),
                    ("email",       "e2e.citizen.<timestamp>@test.waste (unique)"),
                    ("password",    "Test@12345 (>= 8 ký tự)"),
                    ("role",        "citizen"),
                ],
                "returns": "Thông báo 'Đăng ký thành công', redirect vào citizen area",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "CodeceptJS + Playwright"
            },
            {
                "id": "E2E-002B",
                "description": "[E2E] Citizen đăng nhập và thấy form create-report",
                "conditions": [
                    ("email",    "quantranhoang24@gmail.com"),
                    ("password", "Quan1109"),
                    ("route",    "/citizen/create-report"),
                ],
                "returns": "Trang hiển thị 'Tạo Báo Cáo', có input địa chỉ và textarea",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "E2E navigation test"
            },
            {
                "id": "E2E-002C",
                "description": "[E2E – Negative] Submit form trống – thông báo lỗi validation",
                "conditions": [
                    ("address",  "rỗng"),
                    ("image",    "không có"),
                    ("category", "chưa chọn"),
                ],
                "returns": "Thông báo 'Vui lòng điền đầy đủ', form không submit",
                "http_code": 200,
                "type": "A",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "Error guessing – validation guard"
            },
        ]
    },
    {
        "code": "F06",
        "name": "Chức năng E2E – Enterprise & Collector flow",
        "created_by": "Tạ Đức Bảo",
        "executed_by": "Tạ Đức Bảo",
        "lines_of_code": "N/A",
        "test_requirement": "Enterprise đăng nhập và truy cập task management; Collector đăng nhập và truy cập task list; Kiểm tra role-based access control (Collector không vào được enterprise route)",
        "jira_key": "KIEM-16 / KIEM-14",
        "test_cases": [
            {
                "id": "E2E-003A",
                "description": "[E2E] Enterprise đăng nhập và vào dashboard",
                "conditions": [
                    ("email",    "enterprise@test.waste"),
                    ("password", "Enterprise@123"),
                    ("role",     "Enterprise"),
                ],
                "returns": "Login thành công, không thấy 'Email hoặc mật khẩu không đúng'",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "State transition – Enterprise authenticated state"
            },
            {
                "id": "E2E-003B",
                "description": "[E2E – Negative] Enterprise đăng nhập sai mật khẩu",
                "conditions": [
                    ("email",    "enterprise@test.waste"),
                    ("password", "WrongPassword! (SAI)"),
                ],
                "returns": "Thông báo 'Email hoặc mật khẩu không đúng'",
                "http_code": 401,
                "type": "A",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "Error guessing – invalid credentials"
            },
            {
                "id": "E2E-004A",
                "description": "[E2E] Collector đăng nhập và vào dashboard",
                "conditions": [
                    ("email",    "collector@test.waste"),
                    ("password", "Collector@123"),
                    ("role",     "Collector"),
                ],
                "returns": "Login thành công, /collector/dashboard load không lỗi",
                "http_code": 200,
                "type": "N",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "State transition – Collector authenticated"
            },
            {
                "id": "E2E-004B",
                "description": "[E2E – Negative] Collector đăng nhập sai mật khẩu",
                "conditions": [
                    ("email",    "collector@test.waste"),
                    ("password", "InvalidPassword123! (SAI)"),
                ],
                "returns": "Thông báo 'Email hoặc mật khẩu không đúng'",
                "http_code": 401,
                "type": "A",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "Error guessing – invalid password"
            },
            {
                "id": "E2E-004C",
                "description": "[E2E – State Guard] Collector không vào được enterprise route",
                "conditions": [
                    ("email",    "collector@test.waste"),
                    ("password", "Collector@123"),
                    ("route",    "/enterprise/dashboard (FORBIDDEN cho Collector)"),
                ],
                "returns": "Không thấy 'Collector Assignment Management' – bị chặn / redirect",
                "http_code": 403,
                "type": "A",
                "passed": True,
                "date": "2026-06-11",
                "defect": "",
                "note": "Role-based access control – state transition guard"
            },
        ]
    },
]

# ──────────────────────────────────────────────────────────────────────────────
# SHEET BUILDERS
# ──────────────────────────────────────────────────────────────────────────────

def build_function_sheet(ws, func_data):
    """Build one function sheet in UnitestCuaBao format."""
    tc = func_data["test_cases"]
    n_tc = len(tc)
    
    # Column layout:
    # A=label, B=description, C=value/detail, D=cond_label,
    # E..E+n_tc-1 = UTCID columns
    TC_START_COL = 5  # E

    # ── Row 1: Function Code / Name ──
    ws.merge_cells(start_row=1, start_column=1, end_row=1, end_column=4)
    set_cell(ws, 1, 1, "Function Code", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 1, 5, "Function Name", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    ws.merge_cells(start_row=1, start_column=TC_START_COL+1, end_row=1, end_column=TC_START_COL+max(n_tc-1,0)+5)
    set_cell(ws, 1, TC_START_COL+1, func_data["name"], hdr_fill(COLOR_SECTION_BG), bold_font(size=11), left_aligned())
    set_cell(ws, 1, TC_START_COL, func_data["code"], hdr_fill(COLOR_SECTION_BG), bold_font(), centered())

    # ── Row 2: Created By / Executed By ──
    set_cell(ws, 2, 1, "Function Code", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 2, 2, func_data["code"], hdr_fill(COLOR_SECTION_BG), normal_font(), centered())
    set_cell(ws, 2, 3, "Created By", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 2, 4, func_data["created_by"], None, normal_font(), left_aligned())
    set_cell(ws, 2, TC_START_COL, "Executed By", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 2, TC_START_COL+1, func_data["executed_by"], None, normal_font(), left_aligned())

    # ── Row 3: Lines of Code / Jira ──
    set_cell(ws, 3, 1, "Lines of code", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 3, 2, func_data["lines_of_code"], None, normal_font(), centered())
    set_cell(ws, 3, 3, "Jira Key", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 3, 4, func_data["jira_key"], None, normal_font(), centered())

    # ── Row 4: Test Requirement ──
    set_cell(ws, 4, 1, "Test requirement", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    ws.merge_cells(start_row=4, start_column=2, end_row=4, end_column=TC_START_COL+n_tc+3)
    set_cell(ws, 4, 2, func_data["test_requirement"], hdr_fill(COLOR_SECTION_BG), normal_font(), left_aligned(wrap=True))

    # ── Row 5: Summary header ──
    passed = sum(1 for t in tc if t["passed"])
    failed = sum(1 for t in tc if not t["passed"])
    n_types = {"N": 0, "A": 0, "B": 0}
    for t in tc:
        n_types[t["type"]] = n_types.get(t["type"], 0) + 1

    summary_cols = [
        ("Passed", str(passed), COLOR_PASS_BG),
        ("Failed", str(failed), COLOR_FAIL_BG),
        ("Untested", "0", COLOR_NA_BG),
        ("N/A/B", f"N={n_types['N']} A={n_types['A']} B={n_types['B']}", COLOR_NA_BG),
        ("Total Test Cases", str(n_tc), COLOR_SECTION_BG),
    ]
    col = 1
    for label, val, bg in summary_cols:
        set_cell(ws, 5, col, label, hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
        set_cell(ws, 5, col+1, val, hdr_fill(bg), bold_font(), centered())
        col += 2

    # ── Row 6: Empty spacer ──
    ws.row_dimensions[6].height = 4

    # ── Row 7: UTCID header row ──
    set_cell(ws, 7, 1, "Condition", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 7, 2, "Input / State", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 7, 3, "Value / Description", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, 7, 4, "Sub-condition", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    for i, t in enumerate(tc):
        set_cell(ws, 7, TC_START_COL + i, t["id"],
                 hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG, size=9), centered())

    # ── Rows 8+: Condition matrix ──
    # Gather all unique condition labels
    all_cond_labels = []
    seen = set()
    for t in tc:
        for lbl, _ in t["conditions"]:
            if lbl not in seen:
                all_cond_labels.append(lbl)
                seen.add(lbl)

    current_row = 8
    for cond_label in all_cond_labels:
        # Find all distinct values for this condition across test cases
        value_rows = {}  # value -> list of tc indices that use it
        for i, t in enumerate(tc):
            for lbl, val in t["conditions"]:
                if lbl == cond_label:
                    if val not in value_rows:
                        value_rows[val] = []
                    value_rows[val].append(i)

        first_value = True
        for val, tc_indices in value_rows.items():
            if first_value:
                set_cell(ws, current_row, 2, cond_label,
                         hdr_fill(COLOR_COND_LABEL), bold_font(size=9), centered())
                first_value = False
            else:
                set_cell(ws, current_row, 2, "",
                         hdr_fill(COLOR_COND_LABEL), normal_font(), centered())
            set_cell(ws, current_row, 3, val, None, normal_font(size=9), left_aligned())

            for i in range(n_tc):
                if i in tc_indices:
                    set_cell(ws, current_row, TC_START_COL + i, "O",
                             hdr_fill(COLOR_MARK_BG), bold_font(), centered())
                else:
                    set_cell(ws, current_row, TC_START_COL + i, "",
                             None, normal_font(), centered())
            current_row += 1

    # ── Returns / HTTP Status ──
    ret_row = current_row
    set_cell(ws, ret_row, 1, "Confirm", hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
    set_cell(ws, ret_row, 2, "Return", hdr_fill(COLOR_RESULT_BG), bold_font(), centered())
    for i, t in enumerate(tc):
        set_cell(ws, ret_row, TC_START_COL + i, str(t["http_code"]),
                 hdr_fill(COLOR_RESULT_BG), bold_font(), centered())
    current_row += 1

    # Return description
    ret_desc_row = current_row
    set_cell(ws, ret_desc_row, 2, "Expected Result", hdr_fill(COLOR_RESULT_BG), bold_font(), centered())
    for i, t in enumerate(tc):
        set_cell(ws, ret_desc_row, TC_START_COL + i, t["returns"],
                 None, normal_font(size=8), left_aligned(wrap=True))
    ws.row_dimensions[ret_desc_row].height = 45
    current_row += 1

    # ── Execution Section ──
    exec_start = current_row
    labels = ["Description", "Type (N/A/B)", "Passed/Failed", "Executed Date", "Defect ID", "Note"]
    for label in labels:
        set_cell(ws, current_row, 1, "Result" if label == "Description" else "",
                 hdr_fill(COLOR_HEADER_BG), bold_font(color=COLOR_HEADER_FG), centered())
        set_cell(ws, current_row, 2, label, hdr_fill(COLOR_RESULT_BG), bold_font(size=9), centered())
        for i, t in enumerate(tc):
            if label == "Description":
                val = t["description"]
                bg = None
                fnt = normal_font(size=8)
                aln = left_aligned(wrap=True)
                ws.row_dimensions[current_row].height = 40
            elif label == "Type (N/A/B)":
                val = t["type"]
                bg = COLOR_SECTION_BG
                fnt = bold_font()
                aln = centered()
            elif label == "Passed/Failed":
                val = "P" if t["passed"] else "F"
                bg = COLOR_PASS_BG if t["passed"] else COLOR_FAIL_BG
                fnt = bold_font()
                aln = centered()
            elif label == "Executed Date":
                val = t["date"]
                bg = None
                fnt = normal_font(size=9)
                aln = centered()
            elif label == "Defect ID":
                val = t["defect"] or "-"
                bg = None
                fnt = normal_font(size=9)
                aln = centered()
            else:  # Note
                val = t.get("note", "")
                bg = None
                fnt = normal_font(size=8)
                aln = left_aligned(wrap=True)

            set_cell(ws, current_row, TC_START_COL + i, val,
                     hdr_fill(bg) if bg else None, fnt, aln)
        current_row += 1

    # ── Column widths ──
    ws.column_dimensions["A"].width = 10
    ws.column_dimensions["B"].width = 22
    ws.column_dimensions["C"].width = 38
    ws.column_dimensions["D"].width = 12
    for i in range(n_tc):
        col_letter = get_column_letter(TC_START_COL + i)
        ws.column_dimensions[col_letter].width = 18

    ws.freeze_panes = "E8"


def build_summary_sheet(ws, functions):
    """Build Sheet1 – summary of all functions."""
    headers = [
        "Function Code", "Function Name", "Jira Key",
        "Created By", "Executed By",
        "Total TC", "Passed", "Failed",
        "Test Design Technique", "Source File"
    ]
    source_map = {
        "F01": "Controllers/AuthControllerTests.cs",
        "F02": "Application/Reports/CreateReportCommandHandlerTests.cs",
        "F03": "Application/Tasks/AssignCollectorCommandHandlerTests.cs",
        "F04": "Domain/CollectionTaskTests.cs",
        "F05": "e2e/citizen_report_test.js",
        "F06": "e2e/enterprise_assign_test.js + collector_task_test.js",
    }
    technique_map = {
        "F01": "Equivalence Partitioning, Error Guessing",
        "F02": "Boundary Value Analysis, Error Guessing, Equivalence Partitioning",
        "F03": "Decision Table, Role-based Access, Error Guessing",
        "F04": "State Transition Diagram",
        "F05": "End-to-End, Error Guessing",
        "F06": "End-to-End, State Transition Guard, Role-based Access",
    }

    # Title
    ws.merge_cells("A1:J1")
    title_cell = ws.cell(row=1, column=1, value="UNITTEST SUMMARY — WASTE RECYCLING PLATFORM (KCPM)")
    title_cell.fill = hdr_fill(COLOR_HEADER_BG)
    title_cell.font = bold_font(size=14, color=COLOR_HEADER_FG)
    title_cell.alignment = centered()
    title_cell.border = BORDER
    ws.row_dimensions[1].height = 28

    # Sub-title
    ws.merge_cells("A2:J2")
    sub = ws.cell(row=2, column=1, value=f"Generated: {date.today().isoformat()} | Nhóm 5 người | Môn Kiểm Chứng Phần Mềm")
    sub.fill = hdr_fill(COLOR_SECTION_BG)
    sub.font = normal_font(size=10)
    sub.alignment = centered()
    sub.border = BORDER

    # Header row
    for col, h in enumerate(headers, 1):
        cell = ws.cell(row=3, column=col, value=h)
        cell.fill = hdr_fill(COLOR_HEADER_BG)
        cell.font = bold_font(color=COLOR_HEADER_FG)
        cell.alignment = centered(wrap=True)
        cell.border = BORDER
    ws.row_dimensions[3].height = 30

    # Data rows
    for row_idx, func in enumerate(functions, 4):
        tc = func["test_cases"]
        passed = sum(1 for t in tc if t["passed"])
        failed = sum(1 for t in tc if not t["passed"])
        data = [
            func["code"],
            func["name"],
            func["jira_key"],
            func["created_by"],
            func["executed_by"],
            str(len(tc)),
            str(passed),
            str(failed),
            technique_map.get(func["code"], ""),
            source_map.get(func["code"], ""),
        ]
        for col, val in enumerate(data, 1):
            bg = None
            if col == 7:   bg = COLOR_PASS_BG
            elif col == 8: bg = COLOR_FAIL_BG if failed > 0 else COLOR_PASS_BG
            cell = ws.cell(row=row_idx, column=col, value=val)
            if bg: cell.fill = hdr_fill(bg)
            cell.font = normal_font(size=9)
            cell.alignment = left_aligned(wrap=True)
            cell.border = BORDER
        ws.row_dimensions[row_idx].height = 28

    # Column widths
    widths = [12, 38, 14, 18, 18, 8, 8, 8, 38, 38]
    for col, w in enumerate(widths, 1):
        ws.column_dimensions[get_column_letter(col)].width = w

    ws.freeze_panes = "A4"


# ──────────────────────────────────────────────────────────────────────────────
# MAIN
# ──────────────────────────────────────────────────────────────────────────────

def main():
    wb = Workbook()

    # Remove default sheet
    default_sheet = wb.active
    wb.remove(default_sheet)

    # Sheet 1 – Summary
    ws_summary = wb.create_sheet("Sheet1")
    build_summary_sheet(ws_summary, FUNCTIONS)

    # Function sheets
    for func in FUNCTIONS:
        sheet_name = f"Function {func['code']}"
        ws = wb.create_sheet(sheet_name)
        build_function_sheet(ws, func)

    # Save
    out_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "UnitestKCPM.xlsx")
    wb.save(out_path)
    print(f"✅ Generated: {out_path}")
    print(f"   Sheets: Sheet1 (summary) + {len(FUNCTIONS)} function sheets")
    total_tc = sum(len(f['test_cases']) for f in FUNCTIONS)
    total_passed = sum(sum(1 for t in f['test_cases'] if t['passed']) for f in FUNCTIONS)
    print(f"   Total test cases: {total_tc} | Passed: {total_passed} | Failed: {total_tc - total_passed}")


if __name__ == "__main__":
    main()
