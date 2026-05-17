# TC-REP-001: Create Waste Report with Valid Data and Image

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-001 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | Critical |
| **Test Type** | Positive |
| **API Endpoint** | `POST /api/reports/create` |
| **Created Date** | 2025-05-17 |
| **Created By** | <replace with the exact tester name already recorded later in this document> |

## 🎯 Objective

Verify that a Citizen can successfully create a waste report with valid data including location, description, waste category, and image upload.

## ✅ Pre-conditions

1. Backend server running on `http://localhost:8080`
2. Citizen user authenticated (valid `citizenToken`)
3. At least one waste category exists in database (categoryId = 1)
4. Image file available for upload (JPG/PNG, < 5MB)

## 🔧 Test Data

### Request Headers
```
Authorization: Bearer {citizenToken}
Content-Type: multipart/form-data
```

### Request Body (Form Data)
| Field | Value | Type |
|-------|-------|------|
| WasteCategoryId | 1 | text |
| Latitude | 10.7769 | text |
| Longitude | 106.7009 | text |
| Description | "Rác thải sinh hoạt tại vỉa hè" | text |
| Address | "123 Nguyễn Trãi, P.1, Q.1" | text |
| AiSuggestion | "Rác hữu cơ - cần thu gom" | text |
| Images | [test-image.jpg] | file |

## 📝 Test Steps

### Step 1: Authentication Check
1. Verify `citizenToken` is valid and not expired
2. Test token by calling `GET /api/auth/me`

### Step 2: Prepare Report Data
1. Select waste category ID from available categories
2. Get current GPS coordinates (or use test data)
3. Prepare description and address
4. Select test image file (valid format)

### Step 3: Send Create Report Request
1. Open Postman → `03 - Reports > Citizen Reports > POST Create Report`
2. Set Authorization header with `citizenToken`
3. Fill form data fields with test data
4. Attach image file
5. Click "Send"

### Step 4: Verify Response
1. Check HTTP status code
2. Verify response body structure
3. Extract `reportId` from response
4. Save to Postman variable for subsequent tests

### Step 5: Database Verification
1. Query database: `SELECT * FROM waste_reports WHERE id = {reportId}`
2. Verify record exists with correct data
3. Check status = "Pending"
4. Verify image path stored correctly

## ✔️ Expected Results

### Response Status
- **HTTP Code**: `201` (Created) or `200` (OK)
- **Response Time**: < 3000ms (due to image upload)
- **Content-Type**: `application/json`

### Response Body (Actual API Structure)
```json
{
    "message": "Report created successfully",
    "report": {
        "id": "1389e4e0-33b0-4a17-90b4-eb87f1e20b82",
        "citizenId": "5c76be7c-b6bb-49d8-9a78-02ac881064ca",
        "citizenName": "Test User",
        "wasteCategoryId": 1,
        "categoryName": "Rác thải sinh hoạt",
        "description": "Sample waste report from Postman",
        "latitude": 10.7769,
        "longitude": 106.7009,
        "address": "123 Nguyen Trai, District 1",
        "status": "Pending",
        "aiSuggestion": "General waste - manual review needed",
        "createdAt": "2026-05-17T14:35:21Z",
        "imageUrls": [
            "3188d307-cd81-4df5-b8f6-e1daa3df7f5f.png"
        ],
        "rewardPoints": []
    }
}
```

### Database State
- New record in `waste_reports` table
- Status = "Pending"
- Image record in `report_images` table
- `created_at` timestamp populated
- `citizen_id` = citizen user ID

### Postman Variables Updated
- `reportId` = newly created report ID
- Can be used in TC-REP-003, TC-REP-005, etc.

## 🔄 Actual Results

### Execution Date: 2025-05-17

| Metric | Value |
|--------|-------|
| **HTTP Status** | 201 ✅ |
| **Response Time** | 68 ms ✅ |
| **Report Created?** | ✅ Yes |
| **Image Uploaded?** | ✅ Yes (1 image) |
| **Database Record?** | ⬜ Not verified |
| **Postman Variable Set?** | ✅ Yes (reportId captured) |

### Test Results Detail
| Check | Status |
|-------|--------|
| Status code is 200 or 201 | ✅ passed |
| Response has success message | ✅ passed |
| Response time < 3000ms | ✅ passed |
| Report ID exists | ✅ passed (after script fix) |
| Report status is Pending | ✅ passed (after script fix) |
| Image URL exists | ✅ passed (after script fix) |

### Issues Found & Fixed
- **Issue**: Test script expected `json.data.reportId` but API returns `json.report.id`
- **Fix**: Updated test script to match actual API structure
- **Root cause**: Test case documentation outdated (now fixed)

## 📊 Status

⬜ Not Tested | ✅ **Pass** | ⬜ Fail

**Note**: All 6 checks passed after updating Postman test script to match actual API response structure.

## 🐛 Defects (if any)

| Defect ID | Description | Severity | Status |
|-----------|-------------|----------|--------|
| ⬜ | ⬜ | ⬜ | ⬜ |

## 🔗 Related Test Cases

- TC-REP-002: Create report missing fields (Negative)
- TC-REP-003: Get report by ID valid (Positive)
- TC-REP-005: Accept report (Positive)
- TC-AUTH-004: Login to get citizen token (Prerequisite)

## 📝 Notes

- Image formats supported: JPG, PNG, WEBP
- Max file size: Check API documentation (usually 5-10MB)
- Multiple images: Test with 1-3 images if supported
- GPS coordinates: Vietnam range (lat: 8-23, lng: 102-110)
- Concurrent uploads: Consider testing rate limiting

## 🧪 Postman Test Script

```javascript
pm.test("Status code is 200 or 201", () => {
    pm.expect([200, 201]).to.include(pm.response.code);
});

pm.test("Response has success message", () => {
    const json = pm.response.json();
    pm.expect(json.message).to.include("successfully");
});

pm.test("Report ID exists in response", () => {
    const json = pm.response.json();
    const reportId = json.report?.id;
    pm.expect(reportId).to.exist;
    pm.collectionVariables.set("reportId", reportId);
});

pm.test("Report status is Pending", () => {
    const json = pm.response.json();
    const status = json.report?.status;
    pm.expect(status).to.eql("Pending");
});

pm.test("Image URL exists", () => {
    const json = pm.response.json();
    const imageUrls = json.report?.imageUrls;
    pm.expect(imageUrls).to.be.an('array').that.is.not.empty;
});

pm.test("Response time < 3000ms", () => {
    pm.expect(pm.response.responseTime).to.be.below(3000);
});
```

---

**Tested By**: Nguyen Minh Phung **Date**: 2025-05-17

**Reviewed By**: _______________ **Date**: _______________
