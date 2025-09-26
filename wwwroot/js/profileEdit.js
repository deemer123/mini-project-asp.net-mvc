(function(){
  const pic = document.getElementById('avatar');
  const avatarImg = document.getElementById('avatarImg');

  const overlay = document.getElementById('avatarOverlay');
  const fileInput = document.getElementById('avatarFile');  // เปลี่ยนเป็น input file
  const preview  = document.getElementById('avatarPreview');
  const err      = document.getElementById('avatarError');
  const btnOk    = document.getElementById('avatarConfirm');
  const btnCancel= document.getElementById('avatarCancel');
  const hidden   = document.getElementById('avatarUrlHidden');          // ⬅️ hidden input
  const form     = document.querySelector('.profile-form');

  if(!pic || !overlay || !avatarImg || !hidden || !form) return;

  // ตั้งค่าเริ่มต้น = รูปปัจจุบัน (default/เดิม)
  hidden.value = avatarImg.src;

  const open = () => {
    overlay.hidden = false;
    err.textContent = '';
    fileInput.value = '';  // รีเซ็ตค่าไฟล์
    preview.src = avatarImg.src; // แสดงตัวอย่างเป็นรูปเดิมก่อน
    setTimeout(()=> fileInput.focus(), 0);
  };

  const close = () => { overlay.hidden = true; };

  pic.addEventListener('click', open);
  pic.addEventListener('keydown', e=>{
    if(e.key === 'Enter' || e.key === ' '){
      e.preventDefault(); open();
    }
  });

  fileInput.addEventListener('change', function() {
    const file = fileInput.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = function(e) {
        preview.src = e.target.result;
      };
      reader.onerror = () => {
        err.textContent = 'ไม่สามารถโหลดไฟล์ได้';
      };
      reader.readAsDataURL(file);
    }
  });

  function applyFile(){
    const file = fileInput.files[0];
    if (!file) { close(); return; }

    const reader = new FileReader();
    reader.onload = function(e) {
      avatarImg.src = e.target.result;   // เปลี่ยนรูปที่แสดง
      hidden.value  = e.target.result;   // ⬅️ อัปเดตค่าที่จะส่งไปกับฟอร์ม
      close();
    };
    reader.onerror = () => {
      err.textContent = 'ไม่สามารถโหลดไฟล์ได้';
    };
    reader.readAsDataURL(file);
  }

  btnOk.addEventListener('click', applyFile);
  btnCancel.addEventListener('click', close);

  overlay.addEventListener('click', e=>{ if(e.target === overlay) close(); });
  overlay.addEventListener('keydown', e=>{
    if(e.key === 'Escape') close();
    if(e.key === 'Enter')  applyFile();
  });

  // กันพลาด: ก่อน submit ถ้า hidden ว่าง ให้ใช้รูปที่แสดงอยู่
  form.addEventListener('submit', ()=>{
    if(!hidden.value) hidden.value = avatarImg.src;
  });
})();


