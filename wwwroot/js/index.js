(function(){
  const grid   = document.querySelector('.showactivity');
  const track  = document.getElementById('pagerTrack');
  const pager  = document.getElementById('pager');
  if (!grid || !track || !pager) return;

  const items  = [...grid.querySelectorAll('.activity')];
  const prev   = pager.querySelector('[data-dir="-1"]');
  const next   = pager.querySelector('[data-dir="1"]');

  function computePerPage(){
    const w = window.innerWidth;
    if (w >= 1025) return 6;
    if (w >= 768)  return 6;
    return 4;
  }

  let page = 1, totalPages = 1;

  function build(){
    const perPage = computePerPage();
    totalPages = Math.max(1, Math.ceil(items.length / perPage));
    track.innerHTML = '';
    for (let i = 1; i <= totalPages; i++){
      const b = document.createElement('button');
      b.className = 'btn btn--white slider-pill';
      b.textContent = String(i);
      b.dataset.page = String(i);
      b.addEventListener('click', ()=> show(i));
      track.appendChild(b);
    }
    show(1);
  }

  // ✅ เลื่อนขึ้นไปบนสุดของกริด (เผื่อมี navbar ติดบนสุดให้เผื่อ offset)
  function scrollToGridTop(){
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

  function show(p){
    const perPage = computePerPage();
    page = Math.min(Math.max(1, p), totalPages);
    items.forEach((el, idx)=>{
      const visible = idx >= (page-1)*perPage && idx < page*perPage;
      el.style.display = visible ? '' : 'none';
    });
    track.querySelectorAll('.slider-pill').forEach(btn=>{
      btn.classList.toggle('is-active', Number(btn.dataset.page) === page);
    });
    if (prev) prev.disabled = (page === 1);
    if (next) next.disabled = (page === totalPages);

    scrollToGridTop(); // ⬅️ เรียกทุกครั้งที่เปลี่ยนหน้า
  }

  prev?.addEventListener('click', ()=> show(page - 1));
  next?.addEventListener('click', ()=> show(page + 1));

  let to; window.addEventListener('resize', ()=>{
    clearTimeout(to);
    to = setTimeout(build, 150);
  });

  build();
})();
