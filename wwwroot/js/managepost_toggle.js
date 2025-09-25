document.getElementById('toggleBtn').addEventListener('click', function() {
  // Toggle สถานะปุ่ม
  this.classList.toggle('closed');  // สลับ class 'closed' ให้กับปุ่ม
  
  // เปลี่ยนข้อความปุ่ม
  if (this.classList.contains('closed')) {
    this.textContent = "ปิดรับสมัคร";  // เมื่อปิดรับสมัคร
  } else {
    this.textContent = "เปิดรับสมัคร";  // เมื่อเปิดรับสมัคร
  }
});