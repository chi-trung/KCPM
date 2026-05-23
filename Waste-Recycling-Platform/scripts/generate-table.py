import json
import os
import glob
from collections import defaultdict

# Thư mục chứa kết quả Allure
script_dir = os.path.dirname(os.path.abspath(__file__))
results_dir = os.path.join(script_dir, "..", "TestResults", "personal-allure-results")
files = glob.glob(os.path.join(results_dir, "*-result.json"))

tests = []
for f in files:
    with open(f, 'r', encoding='utf-8') as file:
        data = json.load(file)
        
        name = data.get('name', '')
        status = data.get('status', '')
        
        start = data.get('start', 0)
        stop = data.get('stop', 0)
        duration_ms = stop - start
        if duration_ms < 1000:
            duration = f"{duration_ms}ms"
        else:
            s = duration_ms // 1000
            ms = duration_ms % 1000
            duration = f"{s}s {ms}ms"
            
        labels = {l.get('name'): l.get('value') for l in data.get('labels', [])}
        
        parent_suite = labels.get('parentSuite', '')
        suite = labels.get('suite', '')
        feature = labels.get('feature', '')
        story = labels.get('story', '')
        
        # Tự động map Epic dựa vào tên test hoặc tên Suite (Vì AI viết code test quên gán Epic)
        search_str = (name + " " + suite).lower()
        if 'notification' in search_str or 'notify' in search_str or 'unread' in search_str or 'read' in search_str or 'citizenid' in search_str:
            epic = "KIEM-19: SignalR Real-time Tests"
        elif 'reward' in search_str:
            epic = "KIEM-17: Enterprise Collectors & Reward Rules Testing"
        elif 'collector' in search_str or 'setontheway' in search_str:
            epic = "KIEM-14: Collector Module Testing"
        elif 'enterprise' in search_str or 'completetask' in search_str or 'complaint' in search_str or 'task' in search_str:
            epic = "KIEM-18: Enterprise Task Module Testing"
        elif 'auth' in search_str or 'login' in search_str or 'register' in search_str:
            epic = "KIEM-4: Auth Module Testing"
        else:
            epic = "KIEM-14: Collector Module Testing"
        
        tests.append({
            'name': name,
            'status': status,
            'duration': duration,
            'parent_suite': parent_suite,
            'suite': suite,
            'epic': epic,
            'feature': feature,
            'story': story
        })

# Nhóm các test lại theo Epic giống như bạn của bạn
grouped = defaultdict(list)
for t in tests:
    grouped[t['epic']].append(t)

# Tạo file HTML có CSS giả lập y hệt file PDF của bạn kia
html = """
<html>
<head>
<meta charset="utf-8">
<style>
    body { font-family: 'Segoe UI', Calibri, Arial, sans-serif; font-size: 11px; margin: 20px; }
    h2 { text-align: center; font-weight: normal; margin-bottom: 30px; margin-top: 30px; font-size: 18px; }
    table { width: 100%; border-collapse: collapse; }
    th { background-color: #357a58; color: white; text-align: left; padding: 10px 8px; font-weight: bold; border-right: 1px solid white; }
    th:last-child { border-right: none; }
    td { padding: 8px; border-bottom: 1px solid #f0f0f0; }
    .group-header { background-color: #f0f2f5; font-weight: bold; border-top: 2px solid #fff; border-bottom: 2px solid #fff; }
    .group-header td { padding: 12px 8px; }
</style>
</head>
<body>
<h2>Test Report: TÊN CỦA BẠN (Sửa ở file HTML)</h2>
<table>
<thead>
<tr>
    <th>Name</th>
    <th>Status</th>
    <th>Duration</th>
    <th>Parent Suite</th>
    <th>Suite</th>
    <th>Epic</th>
    <th>Feature</th>
    <th>Story</th>
</tr>
</thead>
<tbody>
"""

for epic, t_list in grouped.items():
    epic_name = epic if epic else "No Epic"
    html += f"""
    <tr class="group-header">
        <td colspan="5">Epic: {epic_name}</td>
        <td>Số lượng: {len(t_list)}</td>
        <td></td>
        <td></td>
    </tr>
    """
    for t in t_list:
        html += f"""
        <tr>
            <td>{t['name']}</td>
            <td>{t['status']}</td>
            <td>{t['duration']}</td>
            <td>{t['parent_suite']}</td>
            <td>{t['suite']}</td>
            <td>{t['epic']}</td>
            <td>{t['feature']}</td>
            <td>{t['story']}</td>
        </tr>
        """

html += """
</tbody>
</table>
</body>
</html>
"""

# Lưu ra file HTML ở Desktop cho dễ lấy
output_path = r"C:\Users\gnurt\Desktop\Bao-Cao-Cua-Toi.html"
with open(output_path, "w", encoding='utf-8') as f:
    f.write(html)

print(f"Đã tạo file báo cáo thành công tại: {output_path}")
print("Hãy mở file đó lên bằng trình duyệt Web, bấm Ctrl + P và lưu thành PDF!")
