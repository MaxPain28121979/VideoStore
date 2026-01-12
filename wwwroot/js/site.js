// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
	function disableBtn(btn) {
		if (!btn) return;
		btn.disabled = true;
		if (!btn.dataset._origHtml) btn.dataset._origHtml = btn.innerHTML;
		btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ' + (btn.innerText || btn.dataset._origHtml);
	}

	document.addEventListener('click', function (e) {
		var btn = e.target.closest && e.target.closest('button[type="submit"]');
		if (!btn) return;
		var form = btn.form;
		if (!form) return;
		if (!form.classList.contains('disable-on-submit')) return;
		form._submitButton = btn;
	}, true);

	document.addEventListener('submit', function (e) {
		var form = e.target;
		if (!form || !form.classList || !form.classList.contains('disable-on-submit')) return;
		var btn = form._submitButton || form.querySelector('button[type="submit"]');
		var canDisable = true;
		if (typeof jQuery !== 'undefined' && jQuery && jQuery.fn && jQuery.fn.valid) {
			try { canDisable = jQuery(form).valid(); } catch (err) { }
		} else if (form.checkValidity) {
			canDisable = form.checkValidity();
		}
		if (canDisable) disableBtn(btn);
	}, true);
    
	// Bootstrap modal handling for delete buttons
	var deleteModalEl = document.getElementById('deleteModal');
	if (deleteModalEl) {
		deleteModalEl.addEventListener('show.bs.modal', function (event) {
			var button = event.relatedTarget;
			var id = button.getAttribute('data-id');
			var title = button.getAttribute('data-title');
			var input = deleteModalEl.querySelector('#deleteId');
			var titleEl = deleteModalEl.querySelector('#deleteModalTitle');
			if (input) input.value = id;
			if (titleEl) titleEl.textContent = title;
		});

		// AJAX delete form handling (animated row removal)
		var ajaxForm = deleteModalEl.querySelector('.ajax-delete-form');
		if (ajaxForm) {
			ajaxForm.addEventListener('submit', function (evt) {
				evt.preventDefault();
				var form = evt.target;
				var btn = form.querySelector('button[type="submit"]');
				if (btn) disableBtn(btn);
				var action = form.action;
				var fd = new FormData(form);
				var opts = {
					method: 'POST',
					body: new URLSearchParams(fd),
					headers: { 'X-Requested-With': 'XMLHttpRequest' }
				};
				fetch(action, opts).then(function (res) {
					if (!res.ok) throw new Error('Delete failed');
					// success: animate row
					var id = fd.get('id');
					var row = document.querySelector('tr[data-id="' + id + '"]');
					if (row) {
						row.classList.add('row-fade-out');
						row.addEventListener('animationend', function () { row.remove(); });
					}
					// hide modal
					var bsModal = bootstrap.Modal.getInstance(deleteModalEl);
					if (bsModal) bsModal.hide();
				}).catch(function (err) {
					console.error(err);
					if (btn) { btn.disabled = false; btn.innerHTML = btn.dataset._origHtml || 'Delete'; }
					alert('Unable to delete item. Please try again.');
				});
			});
		}
	}

	// jQuery validation -> Bootstrap validation classes
	if (typeof jQuery !== 'undefined' && jQuery && jQuery.validator) {
		(function ($) {
			$.validator.setDefaults({
				errorElement: 'div',
				errorClass: 'invalid-feedback',
				highlight: function (element) {
					$(element).addClass('is-invalid').removeClass('is-valid');
				},
				unhighlight: function (element) {
					$(element).removeClass('is-invalid').addClass('is-valid');
				},
				errorPlacement: function (error, element) {
					if (element.parent('.input-group').length) {
						error.insertAfter(element.parent());
					} else {
						error.insertAfter(element);
					}
				}
			});
		})(jQuery);
	}


	// Unsaved changes (dirty form) handling and navigation/intercept
	(function () {
		var trackedForms = Array.prototype.slice.call(document.querySelectorAll('form.track-dirty'));
		if (!trackedForms.length) return;

		function setDirty(form, value) {
			form.__isDirty = !!value;
			// optional: add visual cue
			if (value) form.classList.add('has-unsaved'); else form.classList.remove('has-unsaved');
		}

		trackedForms.forEach(function (form) {
			setDirty(form, false);

			// mark clean on submit
			form.addEventListener('submit', function () { setDirty(form, false); });

			// on input/change mark dirty
			form.addEventListener('input', function () { setDirty(form, true); }, true);
			form.addEventListener('change', function () { setDirty(form, true); }, true);

			// add gentle animation class when form first shown
			if (!form.classList.contains('fade-in')) form.classList.add('fade-in');
		});

		// beforeunload browser prompt
		window.addEventListener('beforeunload', function (e) {
			if (trackedForms.some(function (f) { return f.__isDirty; })) {
				var msg = trackedForms[0].getAttribute('data-unsaved-message') || 'You have unsaved changes. Are you sure you want to leave?';
				e.preventDefault();
				e.returnValue = msg;
				return msg;
			}
		});

		// Intercept internal navigation clicks (links) to confirm

		var unsavedModalEl = document.getElementById('unsavedModal');
		var unsavedModalBody = unsavedModalEl && unsavedModalEl.querySelector('#unsavedModalBody');
		var unsavedModalLeave = unsavedModalEl && unsavedModalEl.querySelector('#unsavedModalLeave');
		var unsavedBootstrapModal = unsavedModalEl ? new bootstrap.Modal(unsavedModalEl, { backdrop: 'static' }) : null;
		var pendingNav = null;

		function showUnsavedModal(message, nav) {
			if (!unsavedModalEl) return;
			if (unsavedModalBody) unsavedModalBody.textContent = message || unsavedModalBody.textContent;
			pendingNav = nav || null;
			unsavedBootstrapModal.show();
		}

		function proceedPendingNav() {
			if (!pendingNav) return;
			var nav = pendingNav;
			pendingNav = null;
			if (nav.type === 'link') {
				window.location.href = nav.href;
			} else if (nav.type === 'button') {
				// if button had a data-nav url
				if (nav.href) window.location.href = nav.href;
			}
		}

		// Intercept link clicks
		document.addEventListener('click', function (e) {
			var a = e.target.closest && e.target.closest('a');
			if (!a || a.target === '_blank' || a.hasAttribute('data-bypass')) return;
			if (trackedForms.some(function (f) { return f.__isDirty; })) {
				e.preventDefault();
				var form = trackedForms.find(function (f) { return f.__isDirty; });
				var msg = form.getAttribute('data-unsaved-message') || 'You have unsaved changes. Are you sure you want to leave?';
				showUnsavedModal(msg, { type: 'link', href: a.href });
			}
		}, true);

		// Intercept special navigation buttons
		document.addEventListener('click', function (e) {
			var btn = e.target.closest && e.target.closest('button[data-nav]');
			if (!btn) return;
			if (trackedForms.some(function (f) { return f.__isDirty; })) {
				e.preventDefault();
				var form = trackedForms.find(function (f) { return f.__isDirty; });
				var msg = form.getAttribute('data-unsaved-message') || 'You have unsaved changes. Are you sure you want to leave?';
				var href = btn.getAttribute('data-nav');
				showUnsavedModal(msg, { type: 'button', href: href });
			}
		}, true);

		if (unsavedModalLeave) {
			unsavedModalLeave.addEventListener('click', function () {
				unsavedBootstrapModal.hide();
				setTimeout(proceedPendingNav, 20);
			});
		}

	})();

})();
