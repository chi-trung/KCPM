# BÁO CÁO KIỂM THỬ — COMPLAINTS + COLLECTIONTASK MODULE

## 🧾 1. METADATA (bắt buộc)

| Thuộc tính                   | Giá trị                                                               |
| ---------------------------- | --------------------------------------------------------------------- |
| **Project Name**             | WastePlatform Backend Ecosystem                                       |
| **Sprint**                   | Sprint 3 (Tháng 6/2026)                                               |
| **Task ID**                  | KIEM-67                                                               |
| **Task Title**               | [Sprint-3] Viết báo cáo test Complaints + CollectionTask Module       |
| **Assignee**                 | Thanh Duy                                                             |
| **Reporter**                 | Nguyễn Chí Trung                                                      |
| **Quality Dashboard Links**  | https://chi-trung.github.io/KCPM/                                     |
| **Project Management Board** | https://ut-team-36.atlassian.net/jira/software/projects/KIEM/boards/3 |

---

## MỤC TIÊU VÀ PHẠM VI KIỂM THỬ (TESTING OBJECTIVES & SCOPE)

Việc thiết lập đúng mục tiêu và phạm vi kiểm thử cho hai phân hệ **Complaints Module** (Quản lý khiếu nại của công dân về ô nhiễm) và **CollectionTask Module** (Vòng đời điều phối xe thu gom rác) là điều kiện tiên quyết để đảm bảo chất lượng tổng thể của WastePlatform Backend Ecosystem. Bởi lẽ, Complaints Module và CollectionTask Module không tồn tại độc lập; chúng là hai mắt xích của cùng một chuỗi nghiệp vụ: khi công dân phát sinh khiếu nại, hệ thống cần ghi nhận, phân loại, xử lý và đồng thời tạo ra các ngữ cảnh nghiệp vụ để điều phối hoạt động thu gom/giải quyết. Chỉ cần một sai khác nhỏ ở lớp tích hợp (integration layer) giữa các service, repository, middleware xác thực/xuất quyền, hoặc logic chuyển trạng thái (state transition) cũng có thể dẫn đến hậu quả dây chuyền—từ việc dữ liệu sai lệch, không nhất quán, cho đến nguy cơ rò rỉ thông tin trên Production.

Trong bối cảnh Sprint 3, mục tiêu kiểm thử được ưu tiên theo nguyên tắc “**phòng ngừa lỗi tích hợp trước khi mở rộng kiểm thử theo bề rộng**”. Nghĩa là, thay vì chỉ kiểm tra các hàm riêng lẻ, trọng tâm là xác minh luồng end-to-end ở mức backend: từ API contract đến truy cập dữ liệu và điều phối nghiệp vụ. Với Complaints Module, việc kiểm thử không chỉ dừng lại ở việc “tạo mới khiếu nại thành công”, mà phải chứng minh rằng hệ thống đảm bảo đúng dữ liệu đầu vào, đúng quy tắc nghiệp vụ (ví dụ trạng thái báo cáo, phạm vi xử lý, liên kết với tài nguyên liên quan), và quan trọng hơn là đúng cách hệ thống phản hồi khi gặp các tình huống biên (boundary conditions). Với CollectionTask Module, kiểm thử cần xác nhận rằng việc tạo task, gán/điều phối, cập nhật tiến trình và kết thúc task đều diễn ra chính xác theo logic chuyển trạng thái và đúng quyền người dùng tham gia vận hành.

Tầm quan trọng của **Integration Testing** trở nên đặc biệt rõ ràng khi xét đến bản chất phụ thuộc lẫn nhau giữa Complaints Module và CollectionTask Module. Integration Testing giúp phát hiện các lỗi “không xuất hiện trong unit test” vì unit test thường giả lập bớt các thành phần hoặc cô lập phần code. Trong khi đó, integration test phản ánh đúng cách các lớp hoạt động chung với nhau: API controller truyền tham số xuống service như thế nào; service gọi repository nào; repository ánh xạ dữ liệu ra sao; các sự kiện/notification có được phát sinh đúng thời điểm; và lớp phân quyền (authorization) có chặn đúng hành vi nhạy cảm hay không. Đặc biệt, trong hệ thống có nhiều trạng thái dữ liệu, chỉ cần một chênh lệch nhỏ về mapping trạng thái hoặc thứ tự cập nhật cũng sẽ gây sai lệch theo chuỗi.

Đối với Complaints Module, integration test đóng vai trò xác nhận rằng vòng đời khiếu nại được xử lý đúng trong toàn bộ pipeline. Khi một công dân gửi khiếu nại về tình trạng ô nhiễm, hệ thống phải kiểm tra tính hợp lệ của dữ liệu (validation), chuẩn hóa và lưu trữ thông tin (persistence), sau đó cập nhật trạng thái để phục vụ các bước xử lý tiếp theo. Nếu integration layer xảy ra lỗi, hệ thống có thể ghi nhận sai trạng thái (ví dụ khiếu nại được đánh dấu xử lý trong khi chưa đủ điều kiện), hoặc lưu dữ liệu không đồng bộ giữa bảng chính và bảng liên quan. Những vấn đề này thường không lộ ngay ở unit test vì unit test chỉ kiểm tra logic đơn lẻ. Integration Testing sẽ phơi bày các sai khác do contract giữa các lớp, do thao tác transaction, hoặc do thiếu/không đúng ràng buộc dữ liệu.

Đối với CollectionTask Module, Integration Testing giúp kiểm thử chính xác “**vòng đời điều phối xe thu gom rác**” trong sự phối hợp giữa nhiều tầng. Một collection task thường bao gồm các giai đoạn: tạo task dựa trên ngữ cảnh nghiệp vụ, phân công/điều phối tài nguyên (vehicle, collector), cập nhật tiến độ (in-progress), ghi nhận thời điểm xử lý và hoàn tất (completed), đồng thời đồng bộ dữ liệu để đảm bảo hệ thống hiển thị đúng cho người dùng và các tác vụ tự động khác (ví dụ notification, audit log). Nếu integration test không được thực hiện nghiêm ngặt, rủi ro phát sinh bao gồm: task không được tạo dù điều kiện kích hoạt đã thỏa; task được tạo nhưng dữ liệu liên kết với complaint sai; hoặc trạng thái chuyển đổi không khớp dẫn đến task bị “kẹt” ở trạng thái trung gian. Đây là dạng lỗi có thể gây ảnh hưởng trực tiếp đến vận hành thật.

Một điểm nhấn quan trọng khác là cơ chế **Role-based Access Control (RBAC)** cho nhóm tài khoản **Doanh nghiệp (Enterprise)**. Trong hệ thống WastePlatform Backend Ecosystem, tài khoản Enterprise thường có quyền truy cập và khả năng thao tác vượt trội so với người dùng thông thường. RBAC giúp giới hạn quyền đọc/ghi theo vai trò, đảm bảo rằng mỗi nhóm người dùng chỉ nhìn thấy và thao tác dữ liệu thuộc phạm vi cho phép. Integration Testing vì vậy cần được mở rộng đến phần kiểm thử authorization ở mức tích hợp: không chỉ kiểm tra “endpoint có trả 200/401 đúng”, mà phải xác nhận rằng thông tin trong response không bị rò rỉ theo đường vòng (side channel) hoặc do lỗi query scope.

Việc kiểm thử nghiêm ngặt RBAC cho Enterprise là sống còn nhằm tránh **rò rỉ dữ liệu**. Trong thực tế, rò rỉ dữ liệu có thể xảy ra theo nhiều kịch bản tinh vi: một endpoint được bảo vệ đúng bằng authorization middleware, nhưng bên trong query lại không lọc theo enterpriseId; hoặc dữ liệu được trả về từ repository đúng theo điều kiện, nhưng một số trường nhạy cảm bị include nhầm do mapping DTO; hoặc audit log ghi nhận sai chủ thể (subject) khiến việc truy vết sau này không thể phản chứng. Nếu các kiểm thử chỉ mang tính unit hoặc chỉ kiểm tra quyền ở layer controller, các lỗ hổng kiểu “quên lọc theo scope” vẫn có thể lọt vào Production.

Không chỉ rò rỉ dữ liệu, RBAC còn phải được kiểm thử để ngăn ngừa **xung đột trạng thái dữ liệu trên Production**. Trong hệ thống có stateful workflow, quyền của Enterprise đôi khi liên quan đến việc điều phối hoặc cập nhật tiến trình liên quan đến complaint và collection task. Nếu RBAC không chặt, Enterprise có thể vô tình (hoặc cố ý) thực hiện thao tác cập nhật lên bản ghi không thuộc sở hữu/khả năng quản lý của mình. Điều này dẫn đến tình trạng “hai tác nhân” cùng thao tác vào cùng một bản ghi, tạo ra xung đột trạng thái: complaint có thể bị đánh dấu đã xử lý trong khi task thu gom vẫn chưa được bắt đầu; collection task có thể chuyển sang completed trong khi complaint còn ở trạng thái pending; hoặc hệ thống sinh ra notification mâu thuẫn cho các vai trò khác.

Integration Testing cần tập trung vào cả hai vấn đề—**data confidentiality** và **workflow integrity**—thông qua các kiểm thử tình huống thực tế theo luồng nghiệp vụ. Thay vì kiểm tra RBAC một cách tách rời, kiểm thử nên mô phỏng hành vi Enterprise trong bối cảnh đầy đủ: Enterprise tạo hoặc cập nhật dữ liệu liên quan đến complaint; hệ thống ghi audit log; trigger các cơ chế đồng bộ sang collection task; và cuối cùng đảm bảo rằng kết quả hiển thị cho các vai trò khác là nhất quán. Cách tiếp cận này đảm bảo rằng quyền không chỉ đúng “ở mức cho phép/không cho phép”, mà đúng “ở mức ảnh hưởng nghiệp vụ” (business impact) trên toàn chuỗi.

Ngoài RBAC, Integration Testing còn giúp xác nhận tính đúng đắn của các ràng buộc giữa Complaints Module và CollectionTask Module. Ví dụ, complaint có thể là đầu vào để tạo collection task, do đó bất kỳ sai khác nào trong mapping dữ liệu (complaintId, statusId, serviceArea, time window, hay phân loại ô nhiễm) đều có thể gây sai task. Integration test vì vậy cần được thiết kế theo hướng kiểm thử “**contract liên module**”: đảm bảo rằng contract về payload, status và error handling được duy trì nhất quán. Đồng thời, integration test cần kiểm tra cả những đường lỗi (error path) vì lỗi ở đường lỗi cũng có thể tạo dữ liệu bán phần (partial writes), từ đó làm trạng thái dữ liệu không toàn vẹn.

Trong phạm vi kiểm thử Sprint 3, mục tiêu còn bao gồm việc xác minh độ tin cậy của cơ chế ghi nhận và truy vết như **audit logging** (đặc biệt khi có thao tác từ nhóm Enterprise). Audit log đóng vai trò nền tảng cho điều tra sự cố và kiểm soát tuân thủ. Nếu audit log không ghi đúng actor, endpoint, payload quan trọng hoặc thời điểm, thì ngay cả khi dữ liệu không bị rò rỉ, việc truy trách nhiệm vẫn bị suy giảm. Integration Testing vì vậy nên xác nhận rằng audit log luôn được sinh ra trong các kịch bản nhạy cảm: Enterprise cố gắng đọc dữ liệu ngoài scope; Enterprise cố gắng cập nhật sai trạng thái; hoặc Enterprise kích hoạt chuyển trạng thái kéo theo đồng bộ sang collection task.

Tóm lại, mục tiêu kiểm thử và phạm vi ở chương này nhấn mạnh rằng chất lượng của Complaints Module và CollectionTask Module chỉ đạt chuẩn khi **integration testing** được triển khai như một trục xương sống (backbone). Integration Testing giúp phát hiện lỗi hợp đồng giữa các lớp, sai lệch mapping dữ liệu, thiếu ràng buộc transaction, sai logic chuyển trạng thái, và quan trọng nhất là đảm bảo cơ chế **RBAC cho Enterprise** hoạt động chính xác ở cả lớp truy cập dữ liệu lẫn lớp ảnh hưởng nghiệp vụ. Nhờ đó, hệ thống có thể tránh được hai rủi ro lớn nhất trong Production: **rò rỉ dữ liệu** và **xung đột trạng thái dữ liệu**—đảm bảo tính bảo mật, tính toàn vẹn và tính nhất quán cho toàn bộ chuỗi nghiệp vụ từ khiếu nại đến điều phối thu gom.

---

## CHƯƠNG 1: PHÂN TÍCH CHI TIẾT COMPLAINTS MODULE VỚI DECISION TABLE TESTING

Việc áp dụng **Decision Table Testing** theo tinh thần ISTQB Foundation Level cho hàm **`RespondToComplaint`** nhằm “mổ xẻ” chính xác các tổ hợp điều kiện (conditions) và ánh xạ chúng sang các kết quả hành vi (actions) mà hệ thống mong đợi. Trong kiểm thử doanh nghiệp, cách tiếp cận này đặc biệt quan trọng vì hàm xử lý khiếu nại thường chứa nhiều nhánh nghiệp vụ dựa trên quyền truy cập (Enterprise profile), khả năng chịu lỗi (exception handling), và tính hợp lệ của payload (request DTO). Nếu không hệ thống hóa, tester rất dễ bỏ sót tổ hợp biên dẫn đến lỗi hiển thị sai trạng thái, sai mã HTTP, hoặc tệ hơn là trả về dữ liệu không được phép.

Từ góc nhìn kỹ thuật, `RespondToComplaint` có thể được hiểu như một “bộ điều phối quyết định” (decision engine) nhận vào: (i) ngữ cảnh danh tính/authorization của Enterprise, (ii) dữ liệu request dùng để phản hồi khiếu nại, và (iii) trạng thái nội tại/sự vận hành của các dependency như DB context và mediator/handlers. Do đó, Decision Table được xây dựng theo hướng gom nhóm tất cả các điều kiện đầu vào có ảnh hưởng đến luồng trả kết quả, sau đó xác định duy nhất các action đầu ra. Đây là cách đảm bảo bao phủ logic theo hướng “hệ thống hóa rủi ro” thay vì kiểm thử theo cảm tính.

### 1) Input Conditions (Ba điều kiện đầu vào)

#### C1 (Enterprise Profile Validated)

C1 biểu diễn kết quả của quá trình kiểm tra thực thể **Enterprise** trong `DbContext` dựa trên `ClaimsPrincipal` của `HttpContext`. Về mặt nghiệp vụ, đây không chỉ là “xác thực có token hay không”, mà là bước xác lập **bối cảnh sở hữu dữ liệu** (tenant scope). Khi hệ thống đọc claims (ví dụ enterpriseId) từ JWT/token, sau đó truy vấn `DbContext` để lấy profile Enterprise tương ứng. Nếu profile được tìm thấy và hợp lệ, C1 = True.

Trường hợp profile bị rỗng hoặc lỗi mapping DB xảy ra khi `ClaimsPrincipal` cung cấp enterpriseId nhưng bảng/record Enterprise tương ứng không tồn tại, hoặc mapping giữa model và schema không khớp (ví dụ thay đổi column type, sai tên trường, hoặc query projection làm mất dữ liệu). Trong tình huống này, C1 = False và `RespondToComplaint` phải chuyển sang các hành vi đầu ra phù hợp (thường ưu tiên 401 Unauthorized nếu xét đây là lỗi scope/identity). Với Decision Table, việc mô tả rõ C1 ở trạng thái “rỗng/lỗi mapping” giúp tester hiểu đây là loại lỗi “không thuộc quyền của Enterprise hoặc không thể định danh Tenant”, không phải lỗi validation payload đơn thuần.

C1 cũng là nền tảng để ngăn ngừa rò rỉ dữ liệu: nếu C1 không được xác nhận (Enterprise profile không tồn tại hoặc không khớp), hệ thống tuyệt đối không nên tiếp tục xử lý phản hồi khiếu nại theo hướng truy vấn complaint thuộc tenant khác. Vì vậy, trong bảng quyết định, C1 là điều kiện đầu tiên cần được xét để hạn chế side effects.

#### C2 (System Exception Triggered)

C2 biểu diễn trạng thái “có ngoại lệ hệ thống xảy ra” trong suốt luồng xử lý, bao gồm: giả lập lỗi phần cứng, mất kết nối SQL, timeout truy vấn, lỗi handler từ Mediator (ví dụ handler throw exception, hoặc mediator không resolve handler đúng type). Về mặt kiểm thử, C2 = True khi các tầng phụ thuộc (DB/mediator/infrastructure) tạo ra một **Unexpected Exception**.

Điểm quan trọng là C2 không chỉ mô tả “có exception”, mà mô tả thời điểm và bản chất: exception có thể xảy ra ngay khi truy vấn Enterprise profile, có thể xảy ra khi truy vấn complaint, hoặc xảy ra trong bước thực thi action nghiệp vụ thông qua mediator. Điều này khiến hệ thống cần cơ chế **safe error contract** để đảm bảo response thống nhất và không rò rỉ stack trace.

Nếu C2 = False, nghĩa là không có exception hệ thống bất ngờ; luồng xử lý có thể vẫn trả về 400/401 dựa trên logic nghiệp vụ hoặc validation request. Điều này đảm bảo Decision Table phân biệt rõ: “lỗi nghiệp vụ” (business/data validation) với “lỗi hệ thống” (infrastructure/handler failure).

Để mô phỏng C2 trong kiểm thử tự động, tester có thể áp dụng các kỹ thuật như: cấu hình test server để mock DbContext/connection nhằm throw exception, dùng failpoint để đóng kết nối SQL, hoặc tạo handler throw exception trong pipeline mediator. Việc xác định đúng C2 trong decision table giúp kiểm thử đúng nhánh 500 Internal Server Error và đồng thời kiểm thử contract JSON lỗi.

#### C3 (Request DTO Validity)

C3 biểu diễn tính toàn vẹn và hợp lệ của dữ liệu truyền lên trong request DTO, bao gồm chuỗi `Response` và hai cờ `ResolveImmediately` và `EscalateToAdmin`. C3 = True khi payload thỏa mọi điều kiện validation mà hệ thống đặt ra: ví dụ `Response` không null/không rỗng (hoặc thỏa ràng buộc độ dài/format), `ResolveImmediately` là giá trị boolean hợp lệ (không bị thiếu field gây default sai ngữ nghĩa), và `EscalateToAdmin` phản ánh đúng ý định nghiệp vụ mà hệ thống chờ đợi.

Trường hợp C3 = False xảy ra khi request thiếu trường bắt buộc, chứa giá trị không hợp lệ (ví dụ `Response` rỗng, chứa ký tự vi phạm quy tắc, hoặc vượt giới hạn), hoặc combination logic giữa `ResolveImmediately` và `EscalateToAdmin` không thỏa luật (tùy theo đặc tả nghiệp vụ). Dù Decision Table chỉ liệt kê C3 như một condition tổng quát, mô tả chi tiết các thành phần giúp tester tạo test data rõ ràng và giải thích được tại sao response phải là 400 Bad Request.

Một điểm kỹ thuật đáng lưu ý: tính “hợp lệ DTO” không chỉ là model validation ở lớp controller; nó còn bao gồm xác thực tính toàn vẹn ngữ nghĩa (semantic integrity). Ví dụ, nếu `ResolveImmediately = true` nhưng complaint không cho phép resolve ngay ở trạng thái hiện tại, hệ thống có thể coi đó là business rule violation (có thể trả 400 hoặc 409 tùy thiết kế). Trong phạm vi Decision Table cho `RespondToComplaint`, điều kiện C3 nên tập trung vào validation payload; các business rule khác nếu tồn tại có thể được mô tả như điều kiện mở rộng ở bảng khác hoặc trong phase kiểm thử sâu hơn.

### 2) Expected Actions (Bốn hành động đầu ra kỳ vọng)

Decision Table ánh xạ các tổ hợp C1/C2/C3 sang các action đầu ra. Ở đây, các action được định nghĩa như sau:

- **A1: HTTP 200 OK** — hệ thống chấp nhận phản hồi khiếu nại thành công. Thường xảy ra khi C1 = True (có Enterprise hợp lệ), C2 = False (không có exception hệ thống), và C3 = True (request DTO hợp lệ). Ngoài mã HTTP, tester cũng cần xác nhận body response (ví dụ có thông tin complaint cập nhật, hoặc trạng thái xác nhận) và quan trọng là side effects nghiệp vụ đã diễn ra (trạng thái complaint thay đổi đúng; các liên kết nghiệp vụ nếu có được đồng bộ).

- **A2: HTTP 401 Unauthorized** — hệ thống từ chối do thiếu quyền/không thể xác lập tenant scope. Trong context Decision Table của hàm này, A2 thường gắn với C1 = False (profile enterprise không hợp lệ/không tìm thấy/không mapping được từ claims). Tester phải đảm bảo rằng ngay cả khi C3 hợp lệ (payload đúng), hệ thống vẫn không được phép xử lý vì tenant scope không hợp lệ; đây là nguyên tắc chống cross-tenant access.

- **A3: HTTP 400 Bad Request** — dữ liệu đầu vào không hợp lệ về mặt validation/contract. Thường xảy ra khi C1 = True (đã xác định tenant), C2 = False (không có exception hệ thống), nhưng C3 = False (payload có vấn đề: `Response` rỗng/sai format, hoặc các cờ khiến DTO không hợp lệ). Trong action A3, hệ thống cần trả thông tin lỗi theo chuẩn thống nhất (có thể chứa mã lỗi/chi tiết validation), đồng thời không được trả stack trace.

- **A4: HTTP 500 Internal Server Error** — hệ thống gặp lỗi bất ngờ. Thường xảy ra khi C2 = True (Unexpected Exception Triggered), bất kể C1/C3. Đây là điểm quan trọng trong decision table: lỗi hệ thống vượt qua logic nghiệp vụ và phải chuyển sang nhánh 500 với contract an toàn.

### 3) Phân tích chuyên sâu cơ chế “Safe Response Contract” (ẩn Stack Trace, trả JSON lỗi 500 đồng nhất)

Trong một hệ thống backend doanh nghiệp, cơ chế **Safe Response Contract** là lớp bảo mật và ổn định bắt buộc khi xử lý exception. Khi `RespondToComplaint` gặp một **Unexpected Exception** (ví dụ lỗi mất kết nối SQL, mediator handler throw, hoặc exception do bug nội bộ), hệ thống không được trả lỗi “thô” ra client. Lý do là stack trace và chi tiết internal thường chứa thông tin nhạy cảm hoặc thông tin hữu ích cho tấn công: đường dẫn file, tên lớp/thành phần nội bộ, cấu trúc stack call, hoặc đôi khi lộ cả thông tin giá trị biến (nếu log/exception message vô tình chứa dữ liệu). Kẻ tấn công có thể dùng các thông tin này để suy ra schema, endpoint nội bộ, cấu trúc dependency injection, hoặc pattern xử lý; từ đó tăng khả năng khai thác lỗ hổng.

Vì vậy, khi exception xảy ra, hệ thống cần “bắt” ở tầng middleware/exception handler và chuyển đổi sang một **cấu trúc JSON lỗi 500 đồng nhất**. Tính đồng nhất giúp hai mục tiêu cùng lúc: (i) bảo mật bằng cách không tiết lộ stack trace cho client, và (ii) tạo trải nghiệm vận hành/giám sát tốt hơn vì client và hệ thống frontend có thể dựa vào cùng format để xử lý lỗi (ví dụ hiển thị thông báo, retry hợp lý, hoặc ghi log correlationId).

Ở góc độ kiểm thử, decision table của A4 phải không chỉ kiểm tra status code 500, mà còn kiểm tra nội dung body response theo hợp đồng an toàn. Cụ thể:

- Trường dữ liệu trong JSON lỗi nên thể hiện: `errorCode` hoặc `title` (mã lỗi phân loại), `message` ở mức khái quát (không tiết lộ nguyên nhân kỹ thuật), `requestId/correlationId` để trace log nội bộ, và (nếu có) trường `details` chỉ chứa thông tin không nhạy cảm.
- Bất kỳ chuỗi nào chứa “stack”, “at <namespace>.<class>...”, hoặc đường dẫn file/line number đều phải được loại bỏ.
- Header (ví dụ `Content-Type: application/json`) phải đảm bảo để client không hiểu nhầm.

Nếu hệ thống trả stack trace trực tiếp, các cuộc tấn công khai thác thông tin (information disclosure) có thể xảy ra. Trong môi trường Production nhiều tenant, lộ stack trace còn làm tăng xác suất kẻ tấn công suy ra các điểm yếu theo từng module (Complaints, CollectionTask). Từ đó, chúng có thể xây dựng payload “tối ưu” để khiến hệ thống throw exception theo chủ đích, từ đó lộ dần thông tin. Decision Table testing cho C2 đảm bảo rằng với bất kỳ tổ hợp điều kiện nào có C2 = True, A4 trả về contract an toàn; tức là “exception bất ngờ” không được chuyển đổi thành “leak bất ngờ”.

Một safe contract đồng nhất cũng giúp giảm nhiễu trong việc điều tra sự cố. Tester và engineer vận hành có thể đối chiếu correlationId với log server. Do client chỉ nhận correlationId thay vì stack trace, hệ thống vẫn kiểm soát được kênh thông tin: server lưu stack trace trong log nội bộ (vùng tin cậy), còn client nhận response JSON đã được che giấu (vùng không tin cậy). Đây là mô hình phân tách an toàn điển hình.

Về mặt logic decision, Safe Response Contract khiến A4 trở thành “ưu tiên tuyệt đối” khi C2 xảy ra: dù C1 là True hay False, dù C3 hợp lệ hay không, exception hệ thống vẫn phải trả 500. Việc ưu tiên này tránh các tình huống mâu thuẫn, ví dụ: C1 = False có thể dẫn tới 401, nhưng nếu trong quá trình kiểm tra C1 lại xảy ra lỗi hệ thống (C2 = True) thì hệ thống không được phép vẫn trả 401 vì có thể che mất lỗi hệ thống hoặc trả sai contract. Do đó, bảng quyết định phải phản ánh đúng thứ tự ưu tiên nhánh: Unexpected Exception → 500 safe contract.

Cuối cùng, Safe Response Contract tạo nền tảng cho kiểm thử tự động hóa. Khi format lỗi đồng nhất, tester viết assertions chặt chẽ hơn: kiểm tra schema JSON lỗi, kiểm tra có/không có stack trace, kiểm tra errorCode phù hợp, kiểm tra status code. Điều này giảm flakiness và tăng khả năng duy trì test lâu dài khi backend thay đổi nội bộ (nhưng vẫn giữ contract). Trong môi trường có RBAC cho Enterprise, Safe Response Contract càng quan trọng vì các lỗi hệ thống có thể bị lợi dụng như một tín hiệu (oracle) để suy ra quyền/tenant—nhờ đó làm vỡ các giả định bảo mật.

---

(Phần tiếp theo của báo cáo có thể mở rộng bằng cách dựng decision table dạng ma trận và liệt kê từng rule/tổ hợp C1-C3 tương ứng A1-A4 để đảm bảo cover đầy đủ các nhánh của `RespondToComplaint`.)
