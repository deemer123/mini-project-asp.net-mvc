(function () {
  const bar = document.querySelector('.content1');
  const burger = document.querySelector('.hamburger');
  const actions = document.getElementById('nav-actions');

  // Hamburger (mobile)
  if (burger && bar) {
    burger.addEventListener('click', () => {
      const opened = bar.classList.toggle('menu-open');
      burger.setAttribute('aria-expanded', String(opened));
    });

    // ปิดเมนูเมื่อคลิกลิงก์
    actions?.querySelectorAll('a').forEach(a => {
      a.addEventListener('click', () => {
        bar.classList.remove('menu-open');
        burger.setAttribute('aria-expanded', 'false');
      });
    });

    // รีเซ็ตเมื่อจอกว้างขึ้น
    window.addEventListener('resize', () => {
      if (window.innerWidth >= 769) {
        bar.classList.remove('menu-open');
        burger.setAttribute('aria-expanded', 'false');
      }
    });
  }

  // ===== Dropdowns (แจ้งเตือน / โปรไฟล์)
  const ddNotice  = document.getElementById('noticeDropdown');
  const ddProfile = document.getElementById('profileDropdown');

  function panel(dd){
    return dd?.querySelector('.dropdown-content, .dropdown-content1');
  }
  function setExpanded(dd, expanded){
    const btn = dd?.querySelector('[aria-haspopup="true"]');
    btn?.setAttribute('aria-expanded', String(expanded));
  }
  function closeAll(){
    [ddNotice, ddProfile].forEach(d=>{
      if(!d) return;
      panel(d)?.classList.remove('show');
      setExpanded(d, false);
    });
  }
  function toggle(dd){
    const p = panel(dd);
    const willOpen = !p?.classList.contains('show');
    closeAll();
    if(willOpen){ p?.classList.add('show'); setExpanded(dd, true); }
  }

  // คลิกไอคอน = toggle
  [ddNotice, ddProfile].forEach(dd => {
    if(!dd) return;
    dd.addEventListener('click', e => {
      // ถ้าคลิกภายในเมนู ไม่ต้องปิด
      const isInsidePanel = e.target.closest('.dropdown-content, .dropdown-content1');
      if(isInsidePanel) return;
      toggle(dd);
    });
  });

  // ปิดเมื่อคลิคนอกเมนู
  document.addEventListener('click', e => {
    const isDropdown = e.target.closest('.dropdown');
    if(!isDropdown){ closeAll(); }
  });

  // ปิดด้วย ESC
  document.addEventListener('keydown', e => {
    if(e.key === 'Escape') closeAll();
  });
})();