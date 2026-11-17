(function () {
  'use strict';

  document.querySelectorAll('.rd-alert[data-autodismiss]').forEach(function (el) {
    setTimeout(function () {
      el.style.transition = 'opacity .4s ease, transform .4s ease';
      el.style.opacity = '0';
      el.style.transform = 'translateY(-6px)';
      setTimeout(function () { el.remove(); }, 450);
    }, 7000);
  });

  document.querySelectorAll('input[type="file"][data-preview]').forEach(function (input) {
    input.addEventListener('change', function () {
      var box = document.querySelector(input.dataset.preview);
      if (!box || !input.files || !input.files[0]) return;
      var file = input.files[0];
      if (!/^image\//.test(file.type)) return;

      var img = box.querySelector('img');
      if (!img) {
        img = document.createElement('img');
        img.alt = 'Selected image preview';
        box.appendChild(img);
      }

      img.src = URL.createObjectURL(file);
      box.classList.add('has-image');
    });
  });

  document.querySelectorAll('select[data-autosubmit]').forEach(function (select) {
    select.addEventListener('change', function () {
      if (select.form) select.form.submit();
    });
  });

  var confirmModalEl = document.getElementById('confirmModal');
  if (confirmModalEl && window.bootstrap) {
    var confirmModal = new bootstrap.Modal(confirmModalEl);
    var pendingForm = null;

    document.addEventListener('submit', function (event) {
      var form = event.target;
      if (!(form instanceof HTMLFormElement) || !form.dataset.confirm || form.dataset.confirmed === 'true') return;

      event.preventDefault();
      pendingForm = form;
      confirmModalEl.querySelector('[data-confirm-title]').textContent = form.dataset.confirmTitle || 'Are you sure?';
      confirmModalEl.querySelector('[data-confirm-message]').textContent = form.dataset.confirm;
      var okButton = confirmModalEl.querySelector('[data-confirm-ok]');
      okButton.textContent = form.dataset.confirmOk || 'Yes, continue';
      confirmModal.show();
    });

    confirmModalEl.querySelector('[data-confirm-ok]').addEventListener('click', function () {
      if (!pendingForm) return;
      pendingForm.dataset.confirmed = 'true';
      confirmModal.hide();
      pendingForm.submit();
    });
  }

  document.addEventListener('submit', function (event) {
    var form = event.target;
    if (event.defaultPrevented || !(form instanceof HTMLFormElement)) return;
    var button = form.querySelector('button[type="submit"][data-loading]');
    if (!button) return;
    button.disabled = true;
    button.innerHTML = '<span class="loader-dot"></span>' + (button.dataset.loading || 'Please wait');
  });

  var resultModalEl = document.getElementById('resultModal');
  if (resultModalEl && window.bootstrap) {
    resultModalEl.addEventListener('show.bs.modal', function (event) {
      var trigger = event.relatedTarget;
      if (!trigger) return;
      resultModalEl.querySelector('[name="EnrolmentId"]').value = trigger.dataset.enrolment || '';
      resultModalEl.querySelector('[name="ResultId"]').value = trigger.dataset.result || '0';
      resultModalEl.querySelector('[name="FinishTime"]').value = trigger.dataset.time || '';
      resultModalEl.querySelector('[name="Position"]').value = trigger.dataset.position || '';
      resultModalEl.querySelector('#verifiedSwitch').checked = trigger.dataset.verified === 'true';
      resultModalEl.querySelector('[data-participant]').textContent = trigger.dataset.name || '';
      resultModalEl.querySelector('[data-bib]').textContent = trigger.dataset.bib ? '#' + trigger.dataset.bib : '';
    });
  }

  var editModalEl = document.getElementById('editCategoryModal');
  if (editModalEl && window.bootstrap) {
    editModalEl.addEventListener('show.bs.modal', function (event) {
      var trigger = event.relatedTarget;
      if (!trigger) return;
      editModalEl.querySelector('[name="EventCategoryId"]').value = trigger.dataset.id || '';
      editModalEl.querySelector('[name="CategoryId"]').value = trigger.dataset.category || '';
      editModalEl.querySelector('[name="EntryFee"]').value = trigger.dataset.fee || '';
      editModalEl.querySelector('[name="MaxParticipants"]').value = trigger.dataset.max || '';
      editModalEl.querySelector('[data-category-name]').textContent = trigger.dataset.name || '';
    });
  }
})();
