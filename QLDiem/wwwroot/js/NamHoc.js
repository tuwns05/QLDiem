// NamHoc.js - Tạo các option cho năm học
function loadNamHocOptions() {
    var currentYear = new Date().getFullYear();
    var $select = $("#nam_hoc");
    $select.empty();
    $select.append('<option value="">-- Chọn năm học --</option>');

    // Tạo 5 năm học gần nhất
    for (var i = 0; i < 5; i++) {
        var year = currentYear - i;
        var yearString = (year - 1) + "-" + year;
        $select.append('<option value="' + yearString + '">' + yearString + '</option>');
    }
}

