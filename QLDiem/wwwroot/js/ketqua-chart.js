// ========== KHỞI TẠO BIỂU ĐỒ TRÒN ==========
document.addEventListener('DOMContentLoaded', function () {
    initPieChart();
});

function initPieChart() {
    const pieElement = document.querySelector('.pie');

    if (!pieElement) return;

    // Lấy dữ liệu từ data attributes
    const datPercent = parseFloat(pieElement.getAttribute('data-dat')) || 0;
    const rotPercent = parseFloat(pieElement.getAttribute('data-rot')) || 0;

    // Validate dữ liệu
    const totalPercent = datPercent + rotPercent;
    if (totalPercent > 100) {
        console.warn('Tổng phần trăm vượt quá 100%');
        return;
    }

    // Set CSS variable cho biểu đồ
    pieElement.style.setProperty('--dat-percent', datPercent);

    // Tạo gradient động
    const gradient = `conic-gradient(
        #48bb78 0deg,
        #48bb78 ${datPercent * 3.6}deg,
        #f56565 ${datPercent * 3.6}deg,
        #f56565 360deg
    )`;

    pieElement.style.background = gradient;

    // Animation khi load
    animatePieChart(pieElement, datPercent);
}

// ========== ANIMATION BIỂU ĐỒ ==========
function animatePieChart(element, targetPercent) {
    let currentPercent = 0;
    const duration = 1500; // 1.5 giây
    const frameDuration = 1000 / 60; // 60fps
    const totalFrames = Math.round(duration / frameDuration);
    const percentPerFrame = targetPercent / totalFrames;

    let frame = 0;

    const animate = () => {
        frame++;
        currentPercent = Math.min(currentPercent + percentPerFrame, targetPercent);

        // Cập nhật gradient
        const gradient = `conic-gradient(
            #48bb78 0deg,
            #48bb78 ${currentPercent * 3.6}deg,
            #f56565 ${currentPercent * 3.6}deg,
            #f56565 360deg
        )`;

        element.style.background = gradient;

        if (frame < totalFrames) {
            requestAnimationFrame(animate);
        }
    };

    requestAnimationFrame(animate);
}

// ========== CẬP NHẬT BIỂU ĐỒ ĐỘNG ==========
function updatePieChart(datPercent, rotPercent, siSo, svDat, svRot) {
    const pieElement = document.querySelector('.pie');
    const pieCenter = document.querySelector('.pie-center');
    const chartText = document.querySelector('.chart-text');

    if (!pieElement) return;

    // Cập nhật thuộc tính data
    pieElement.setAttribute('data-dat', datPercent);
    pieElement.setAttribute('data-rot', rotPercent);

    // Cập nhật hiển thị sĩ số
    if (pieCenter) {
        pieCenter.innerHTML = `
            <b>${siSo}</b>
            <small>SV</small>
        `;
    }

    // Cập nhật text thống kê
    if (chartText) {
        chartText.innerHTML = `
            <p>Sĩ số: <b>${siSo}</b></p>
            <p class="dat">● ${svDat} đạt (${datPercent}%)</p>
            <p class="rot">● ${svRot} rớt (${rotPercent}%)</p>
        `;
    }

    // Animate lại biểu đồ
    animatePieChart(pieElement, datPercent);
}

// ========== VÍ DỤ SỬ DỤNG ==========
// Gọi hàm này từ code C# hoặc khi có dữ liệu mới
// updatePieChart(75, 25, 120, 90, 30);

// ========== XỬ LÝ RESPONSIVE ==========
window.addEventListener('resize', debounce(() => {
    initPieChart();
}, 250));

// Debounce helper
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}