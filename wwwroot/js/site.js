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
		document.addEventListener('click', function (e) {
			var a = e.target.closest && e.target.closest('a');
			if (!a || a.target === '_blank' || a.hasAttribute('data-bypass')) return;
			if (trackedForms.some(function (f) { return f.__isDirty; })) {
				var form = trackedForms.find(function (f) { return f.__isDirty; });
				var msg = form.getAttribute('data-unsaved-message') || 'You have unsaved changes. Are you sure you want to leave?';
				if (!confirm(msg)) {
					e.preventDefault();
					return false;
				}
			}
		}, true);

		// Also intercept clicks on buttons that would navigate (like our Cancel anchors rendered as links)
		document.addEventListener('click', function (e) {
			var btn = e.target.closest && e.target.closest('button[data-nav]');
			if (!btn) return;
			if (trackedForms.some(function (f) { return f.__isDirty; })) {
				var form = trackedForms.find(function (f) { return f.__isDirty; });
				var msg = form.getAttribute('data-unsaved-message') || 'You have unsaved changes. Are you sure you want to leave?';
				if (!confirm(msg)) {
					e.preventDefault();
					return false;
				}
			}
		}, true);

	})();

})();
