$(document).ready(function () {
    function loadLopHocPhan() {
        var hocKy = $("#hoc_ky").val();
        var namHoc = $("#nam_hoc").val();

        if (hocKy && namHoc) {
            $.ajax({
                url: '/Admin/GetLopHocPhanByHocKyNamHoc',
                type: 'GET',
                data: {
                    hocKy: parseInt(hocKy),
                    namHoc: namHoc
                },
                success: function (data) {
                    console.log("Data received:", data);
                    var $select = $("#maLopHp"); // Sửa từ "#MaHp" thành "#maLopHp"
                    $select.empty();
                    $select.append('<option value="">-- Chọn lớp học phần --</option>');

                    $.each(data, function (i, hp) {
                        $select.append('<option value="' + hp.maLopHp + '">' + hp.maLopHp + ' - ' + hp.tenHp + '</option>');
                    });
                },
                error: function (xhr, status, error) {
                    console.log("Error:", error);
                    var $select = $("#maLopHp");
                    $select.empty();
                    $select.append('<option value="">-- Lỗi khi tải dữ liệu --</option>');
                }
            });
        } else {
            var $select = $("#maLopHp");
            $select.empty();
            $select.append('<option value="">-- Chọn lớp học phần --</option>');
        }
    }

    // Gọi hàm khi thay đổi học kỳ hoặc năm học
    $("#hoc_ky, #nam_hoc").change(loadLopHocPhan);

    // Khởi tạo giá trị năm học khi trang được tải
    loadNamHocOptions();
});