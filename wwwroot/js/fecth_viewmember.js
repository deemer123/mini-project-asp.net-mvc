(async function () {
  const overlay = document.getElementById('memberOverlay');
  const bodySlot = document.getElementById('overlay-body');

  // ฟังก์ชั่นเปิด overlay และโหลดเนื้อหาจาก viewmember
  async function openOverlay(href) {
    try {
      const res = await fetch(href);
      const html = await res.text();
      const doc = new DOMParser().parseFromString(html, 'text/html');
      const main = doc.querySelector('main');
      bodySlot.innerHTML = main ? main.innerHTML : '<p>ไม่พบข้อมูล</p>';
      
      console.log(res);
      console.log(html);
      
      // ทำให้ตารางเลื่อนในกรอบ
      bodySlot.querySelectorAll('table').forEach(t => {
        const wrap = document.createElement('div');
        wrap.classList.add('table-scroll');
        t.parentNode.insertBefore(wrap, t);
        wrap.appendChild(t);
      });

      // แสดง overlay
      overlay.hidden = false;
      document.body.style.overflow = 'hidden';
    } catch (e) {
      bodySlot.innerHTML = '<p>โหลดข้อมูลไม่สำเร็จ</p>';
      overlay.hidden = false;
    }
  }

  // ปิด overlay
  function closeOverlay() {
    overlay.hidden = true;
    bodySlot.innerHTML = '';
    document.body.style.overflow = '';
  }

  // เปิดจากปุ่ม "ดูสมาชิก"
  document.querySelectorAll('.view-members').forEach(btn => {
    btn.addEventListener('click', () => openOverlay(btn.dataset.href || 'viewmember.html'));
  });

  // ปิดเมื่อคลิกฉากหลัง
  overlay.addEventListener('click', e => {
    if (e.target.matches('[data-close]')) closeOverlay();
  });

  // ปิดเมื่อกด ESC
  document.addEventListener('keydown', e => {
    if (!overlay.hidden && e.key === 'Escape') closeOverlay();
  });
})();
