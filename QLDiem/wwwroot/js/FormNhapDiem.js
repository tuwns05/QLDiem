function openNhapDiem(maSv, maHp, hocKy, namHoc) {
    // Gán giá trị vào các input hidden
    document.getElementById("MaSv").value = maSv;
    document.getElementById("MaHp").value = maHp;
    document.getElementById("hocKy").value = hocKy;
    document.getElementById("namHoc").value = namHoc;

    // Clear input điểm
    document.getElementById("DiemQt").value = "";
    document.getElementById("DiemCk").value = "";

    // Show modal
    const modalEl = document.getElementById('nhapDiemModal');
    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}