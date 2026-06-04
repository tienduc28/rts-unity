# Project Overview: Unity RTS

Đây là một dự án game chiến thuật thời gian thực (RTS) được phát triển bằng Unity.

## Cấu trúc dự án
- `Assets/Scripts/Player`: Chứa logic điều khiển của người chơi, bao gồm `PlayerInput.cs` để xử lý đầu vào và `CameraConfig.cs` cho cấu hình camera.
- `Assets/Scripts/Units`: Chứa logic cho các đơn vị trong game.
    - `ISelectable.cs`: Interface cho các đối tượng có thể chọn được.
    - `Worker.cs`: Logic cụ thể cho đơn vị công nhân.
    - `HoldGunIK.cs`: Xử lý Inverse Kinematics cho việc cầm súng của đơn vị.

## Chỉ dẫn cho Junie
- **Ngôn ngữ**: Sử dụng tiếng Việt cho tất cả các phản hồi.
- **Kiểm tra**: Chạy thử các test (nếu có) để đảm bảo tính chính xác của giải pháp.
- **Build**: Kiểm tra khả năng build của dự án trước khi submit nếu có thay đổi quan trọng về cấu trúc hoặc dependency.
- **Code Style**: Tuân thủ phong cách lập trình hiện tại của dự án (C# standard cho Unity).