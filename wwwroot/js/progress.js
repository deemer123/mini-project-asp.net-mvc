document.addEventListener("DOMContentLoaded", function () {
      const progressText = document.getElementById('progressText').textContent;
      const match = progressText.match(/(\d+)\s*\/\s*(\d+)/);

      if (match) {
        const current = parseInt(match[1], 10);
        const total = parseInt(match[2], 10);
        const percent = (current / total) * 100;
        document.getElementById('progressFill').style.width = percent + '%';
      } else {
        console.warn("ไม่พบรูปแบบตัวเลขในข้อความ");
      }
    });
