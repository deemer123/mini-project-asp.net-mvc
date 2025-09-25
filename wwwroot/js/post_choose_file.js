let previousImageSrc = "";  // เก็บค่าภาพเดิมไว้ที่นี้

document.querySelector('.edit-button').addEventListener('click', function() {
  // เก็บค่า src ปัจจุบันของภาพก่อนที่จะเปลี่ยน
  const currentImage = document.querySelector('.addpost-image');
  previousImageSrc = currentImage.src;  // เก็บ src เดิมไว้
  document.getElementById('activity_imgOverlay').classList.add('active');
});

document.getElementById('activity_imgCancel').addEventListener('click', function() {
  // กดยกเลิก: รีเซ็ตค่า src กลับไปเป็นค่าภาพเดิม
  const currentImage = document.querySelector('.addpost-image');
  currentImage.src = previousImageSrc;  // ใช้ค่า src เดิมที่เก็บไว้
  document.getElementById('activity_imgOverlay').classList.remove('active');
});

document.getElementById('activity_imgFile').addEventListener('change', function(e) {
  const file = e.target.files[0];
  if (file) {
    const reader = new FileReader();
    reader.onload = function(event) {
      document.querySelector('.addpost-image').src = event.target.result; // เปลี่ยนภาพที่แสดง
      document.getElementById('imageUrlHidden').value = event.target.result; // เก็บค่า URL ของภาพ
    };
    reader.readAsDataURL(file);
  }
});

document.getElementById('activity_imgConfirm').addEventListener('click', function() {
  document.getElementById('activity_imgOverlay').classList.remove('active');
});
