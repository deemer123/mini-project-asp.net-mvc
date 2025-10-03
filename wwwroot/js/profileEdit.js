// profileEdit.js
(function(){
  // หา elements
  const pic = document.getElementById('avatar');
  const avatarImg = document.getElementById('avatarImg');
  const overlay = document.getElementById('avatarOverlay');
  const fileInput = document.getElementById('avatarFile');
  const preview  = document.getElementById('avatarPreview');
  const err      = document.getElementById('avatarError');
  const btnOk    = document.getElementById('avatarConfirm');
  const btnCancel= document.getElementById('avatarCancel');
  const hidden   = document.getElementById('avatarUrlHidden');
  const form     = document.querySelector('.profile-form');

  // Debug: log if any missing
  const missing = [];
  if(!pic) missing.push('avatar');
  if(!avatarImg) missing.push('avatarImg');
  if(!overlay) missing.push('avatarOverlay');
  if(!fileInput) missing.push('avatarFile');
  if(!preview) missing.push('avatarPreview');
  if(!err) missing.push('avatarError');
  if(!btnOk) missing.push('avatarConfirm');
  if(!btnCancel) missing.push('avatarCancel');
  if(!hidden) missing.push('avatarUrlHidden');
  if(!form) missing.push('.profile-form');

  if (missing.length) {
    console.warn('profileEdit: missing elements ->', missing.join(', '));
    // don't return: still try to attach partial functionality if possible
  }

  // helper to safely set overlay visible/hidden using class
  function openOverlay(){
    if(!overlay) return;
      overlay.hidden = false;
      err.textContent = '';
      fileInput.value = '';  // รีเซ็ตค่าไฟล์
      preview.src = avatarImg.src; // แสดงตัวอย่างเป็นรูปเดิมก่อน
      setTimeout(()=> fileInput.focus(), 0);
  }
  function closeOverlay(){
    overlay.hidden = true;
  }

  // attach events if elements exist
  if(pic) {
    pic.addEventListener('click', (e)=> {
      e.preventDefault();
      console.log("✅ Avatar clicked"); // เพิ่มบรรทัดนี้
      openOverlay();
    });
    pic.addEventListener('keydown', e => {
      if(e.key === 'Enter' || e.key === ' ') { e.preventDefault(); openOverlay(); }
    });
  }

  if(fileInput){
    fileInput.addEventListener('change', function() {
      const file = fileInput.files && fileInput.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = function(e) {
          if(preview) preview.src = e.target.result;
        };
        reader.onerror = () => {
          if(err) err.textContent = 'ไม่สามารถโหลดไฟล์ได้';
          console.error('profileEdit: FileReader error');
        };
        reader.readAsDataURL(file);
      }
    });
  }

  function applyFile(){
    const file = fileInput.files[0];
    if (!file) { closeOverlay(); return; }

    const reader = new FileReader();
    reader.onload = function(e) {
      avatarImg.src = e.target.result;   // เปลี่ยนรูปที่แสดง
      console.log(hidden);
      console.log(e.target.result);
      hidden.value  = e.target.result;   // ⬅️ อัปเดตค่าที่จะส่งไปกับฟอร์ม
      closeOverlay();
    };
    reader.onerror = () => {
      err.textContent = 'ไม่สามารถโหลดไฟล์ได้';
    };
    reader.readAsDataURL(file);
  }

  if(btnOk) btnOk.addEventListener('click', applyFile);
  if(btnCancel) btnCancel.addEventListener('click', (e) => { e.preventDefault(); closeOverlay(); });

  if(overlay){
    overlay.addEventListener('click', e=>{ if(e.target === overlay) closeOverlay(); });
    overlay.addEventListener('keydown', e=>{
      if(e.key === 'Escape') closeOverlay();
      if(e.key === 'Enter') applyFile();
    });
  }

  // before submit ensure hidden has some value (fallback)
  if(form){
    form.addEventListener('submit', ()=> {
      if (hidden && !hidden.value && avatarImg) hidden.value = avatarImg.src || '';
    });
  }

  // final debug info
  console.log('profileEdit: initialized (missing: ' + missing.length + ')');
})();
