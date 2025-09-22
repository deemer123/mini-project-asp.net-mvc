// === เปลี่ยนรูปโปรไฟล์ด้วย URL และส่งค่าไปกับฟอร์ม ===
(function(){
  const pic = document.getElementById('avatar');
  const avatarImg = document.getElementById('avatarImg');

  const overlay = document.getElementById('avatarOverlay');
  const urlInput = document.getElementById('avatarUrl');
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
    urlInput.value = '';
    preview.src = avatarImg.src; // แสดงตัวอย่างเป็นรูปเดิมก่อน
    setTimeout(()=> urlInput.focus(), 0);
  };
  const close = () => { overlay.hidden = true; };

  pic.addEventListener('click', open);
  pic.addEventListener('keydown', e=>{
    if(e.key === 'Enter' || e.key === ' '){
      e.preventDefault(); open();
    }
  });

  urlInput.addEventListener('input', ()=>{
    const v = urlInput.value.trim();
    preview.src = v || avatarImg.src;
    err.textContent = '';
  });

  function applyUrl(){
    const v = urlInput.value.trim();

    // ถ้าไม่กรอกอะไรเลย: ใช้ค่าเดิม/ค่า default (แค่ปิดกล่อง)
    if(!v){ close(); return; }

    // ทดสอบโหลดก่อน ค่อยอัปเดตจริง
    const test = new Image();
    test.onload = () => {
      avatarImg.src = v;     // เปลี่ยนรูปที่แสดง
      hidden.value  = v;     // ⬅️ อัปเดตค่าที่จะส่งไปกับฟอร์ม
      close();
    };
    test.onerror = () => {
      err.textContent = 'โหลดรูปไม่สำเร็จ ตรวจสอบ URL อีกครั้ง';
    };
    test.src = v;
  }

  btnOk.addEventListener('click', applyUrl);
  btnCancel.addEventListener('click', close);

  overlay.addEventListener('click', e=>{ if(e.target === overlay) close(); });
  overlay.addEventListener('keydown', e=>{
    if(e.key === 'Escape') close();
    if(e.key === 'Enter')  applyUrl();
  });

  // กันพลาด: ก่อน submit ถ้า hidden ว่าง ให้ใช้รูปที่แสดงอยู่
  form.addEventListener('submit', ()=>{
    if(!hidden.value) hidden.value = avatarImg.src;
  });
})();
