// js/confirm.js
(() => {
  // สร้าง modal ครั้งเดียวต่อเพจ
  function ensureModal() {
    let overlay = document.getElementById('confirmOverlay');
    if (overlay) return overlay;

    overlay = document.createElement('div');
    overlay.id = 'confirmOverlay';
    overlay.className = 'confirm-overlay';
    overlay.innerHTML = `
      <div class="confirm-modal" role="dialog" aria-modal="true" aria-labelledby="confirmTitle">
        <h3 id="confirmTitle" class="confirm-title">ยืนยันการทำรายการ</h3>
        <p class="confirm-msg" id="confirmMsg">คุณแน่ใจหรือไม่?</p>
        <div class="confirm-actions">
          <button type="button" class="btn bg_white" id="confirmCancel">ยกเลิก</button>
          <button type="button" class="btn"  id="confirmOk">ยืนยัน</button>
        </div>
      </div>
    `;
    document.body.appendChild(overlay);
    return overlay;
  }

  // คืนโฟกัสกลับปุ่มเดิมเมื่อปิด
  let lastActive = null;

  // Promise-based confirm ใช้ซ้ำได้
  window.confirmDialog = function (opts = {}) {
    const {
      title = 'ยืนยันการทำรายการ',
      message = 'คุณแน่ใจหรือไม่?',
      okText = 'ยืนยัน',
      cancelText = 'ยกเลิก',
    } = opts;

    const overlay = ensureModal();
    const modal   = overlay.querySelector('.confirm-modal');
    const elTitle = overlay.querySelector('#confirmTitle');
    const elMsg   = overlay.querySelector('#confirmMsg');
    const btnOk   = overlay.querySelector('#confirmOk');
    const btnNo   = overlay.querySelector('#confirmCancel');

    elTitle.textContent = title;
    elMsg.textContent   = message;
    btnOk.textContent   = okText;
    btnNo.textContent   = cancelText;

    overlay.classList.add('is-open');
    lastActive = document.activeElement;
    btnOk.focus();

    return new Promise((resolve) => {
      const close = (result) => {
        overlay.classList.remove('is-open');
        // คืนโฟกัส
        setTimeout(() => { try { lastActive?.focus(); } catch {} }, 0);
        cleanup();
        resolve(result);
      };
      const onKey = (e) => {
        if (e.key === 'Escape') close(false);
        if (e.key === 'Enter')  close(true);
      };
      const onOutside = (e) => { if (e.target === overlay) close(false); };

      function cleanup() {
        btnOk.removeEventListener('click', onOk);
        btnNo.removeEventListener('click', onNo);
        overlay.removeEventListener('click', onOutside);
        document.removeEventListener('keydown', onKey);
      }
      const onOk = () => close(true);
      const onNo = () => close(false);

      btnOk.addEventListener('click', onOk);
      btnNo.addEventListener('click', onNo);
      overlay.addEventListener('click', onOutside);
      document.addEventListener('keydown', onKey, { once: false });
    });
  };

  // Delegation: ปุ่มที่มี data-confirm จะถูกถามก่อนเสมอ
  // รองรับทั้ง .btn.cancal (สะกดเดิม) และ [data-confirm]
  document.addEventListener('click', async (e) => {
    const el = e.target.closest('[data-confirm], .btn.cancal, .btn.cancel');
    if (!el) return;

    // ถ้าไม่มีข้อความ ตั้งค่า default ตามชนิด
    const msg = el.dataset.confirm || 'ต้องการยกเลิกกิจกรรมนี้ใช่หรือไม่?';
    e.preventDefault();

    const ok = await window.confirmDialog({
      title: el.dataset.confirmTitle || 'ยืนยันการยกเลิก',
      message: msg,
      okText: el.dataset.okText || 'ยืนยัน',
      cancelText: el.dataset.cancelText || 'ยกเลิก',
    });
    if (!ok) return;

    // กดยืนยันแล้ว เดินหน้าทำงานเดิม:
    // 1) ถ้าเป็นลิงก์ → ไปหน้าเป้าหมาย
    if (el.tagName === 'A' && el.href) {
      window.location.href = el.href;
      return;
    }
    // 2) ถ้าเป็นปุ่ม submit ในฟอร์ม → submit ฟอร์ม
    const form = el.closest('form');
    if (el.type === 'submit' && form) {
      form.requestSubmit(el);
      return;
    }
    // 3) อย่างอื่น → ยิงอีเวนต์ให้ผู้ใช้ผูก handler เอง
    el.dispatchEvent(new CustomEvent('confirm:accepted', { bubbles: true }));
  });
})();
// ใส่แอตทริบิวต์ data-confirm บนปุ่ม/ลิงก์ที่จะทำงานอันตราย:

// <button class="btn cancal" data-confirm="ต้องการยกเลิกกิจกรรมนี้ใช่ไหม?">ยกเลิกกิจกรรม</button>


// หรือ

// <a href="delete.html" class="btn cancel" data-confirm="ลบข้อมูลนี้ถาวร?">ลบ</a>

// ปรับข้อความปุ่ม/หัวเรื่องเฉพาะจุด (ถ้าอยาก)
// <button
//   class="btn cancal"
//   data-confirm="ต้องการยกเลิกกิจกรรมวันที่ 10 พ.ย. ใช่ไหม?"
//   data-confirm-title="ยืนยันการยกเลิก"
//   data-ok-text="ยืนยัน"
//   data-cancel-text="ปิด"
// >ยกเลิกกิจกรรม</button>