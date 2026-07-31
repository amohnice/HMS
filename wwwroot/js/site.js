// ============================================================
// HMS client behaviour
// ============================================================

(function () {
    'use strict';

    // ------------------------------------------------------------
    // Side drawer
    // ------------------------------------------------------------
    // Opens an ordinary Create/Edit page inside a drawer by lifting the content
    // out of that page's <main class="surface">. No dedicated endpoints needed,
    // and with JS off the links stay ordinary navigations.
    var Drawer = {
        el: null, backdrop: null, body: null, titleEl: null, lastFocus: null,

        init: function () {
            this.el = document.getElementById('drawer');
            this.backdrop = document.getElementById('drawerBackdrop');
            this.body = document.getElementById('drawerBody');
            this.titleEl = document.getElementById('drawerTitle');
            if (!this.el) return;

            var self = this;

            document.getElementById('drawerClose')?.addEventListener('click', function () { self.close(); });
            this.backdrop?.addEventListener('click', function () { self.close(); });

            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape' && self.isOpen()) self.close();
            });

            // Delegated, so links rendered later still work.
            document.addEventListener('click', function (e) {
                var link = e.target.closest('[data-drawer]');
                if (!link) return;
                var href = link.getAttribute('href');
                if (!href || href === '#') return;
                e.preventDefault();
                self.open(href, link.getAttribute('data-drawer-title') || link.textContent.trim());
            });
        },

        isOpen: function () { return !!this.el && this.el.classList.contains('is-open'); },

        open: function (href, title) {
            this.lastFocus = document.activeElement;
            this.titleEl.textContent = title || 'Details';
            this.body.innerHTML = '<div class="drawer-loading">Loading…</div>';
            this.el.classList.add('is-open');
            this.el.setAttribute('aria-hidden', 'false');
            this.backdrop.classList.add('is-visible');
            document.body.classList.add('has-drawer-open');
            this.load(href);
        },

        close: function () {
            if (!this.el) return;
            this.el.classList.remove('is-open');
            this.el.setAttribute('aria-hidden', 'true');
            this.backdrop.classList.remove('is-visible');
            document.body.classList.remove('has-drawer-open');
            if (this.lastFocus) this.lastFocus.focus();
        },

        load: function (href) {
            var self = this;
            fetch(href, { headers: { 'X-Requested-With': 'fetch' }, credentials: 'same-origin' })
                .then(function (res) {
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    return res.text();
                })
                .then(function (html) { self.inject(html); })
                .catch(function () {
                    // Never trap the user in a broken drawer — fall back to the real page.
                    window.location.href = href;
                });
        },

        // Lift the content out of a full page response and drop its heading:
        // the drawer header already says what this is.
        inject: function (html) {
            var doc = new DOMParser().parseFromString(html, 'text/html');
            var content = doc.querySelector('main.surface') || doc.body;

            content.querySelectorAll('.surface-head, .page-header, .dashboard-header')
                .forEach(function (n) { n.remove(); });

            this.body.innerHTML = content.innerHTML;
            this.bindForm();

            var first = this.body.querySelector('input:not([type=hidden]):not([type=submit]), select, textarea');
            if (first) first.focus();
        },

        bindForm: function () {
            var self = this;
            var form = this.body.querySelector('form');
            if (!form) return;

            form.addEventListener('submit', function (e) {
                e.preventDefault();
                var submit = form.querySelector('[type=submit]');
                if (submit) submit.disabled = true;

                fetch(form.getAttribute('action') || window.location.href, {
                    method: (form.getAttribute('method') || 'post').toUpperCase(),
                    body: new FormData(form),
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'fetch' }
                })
                    .then(function (res) {
                        // MVC redirects on success and re-renders the form on validation
                        // failure, so `redirected` is the accept/reject signal.
                        if (res.redirected) {
                            window.location.reload();
                            return null;
                        }
                        return res.text();
                    })
                    .then(function (html) {
                        if (html !== null) self.inject(html);   // show the validation errors
                    })
                    .catch(function () {
                        if (submit) submit.disabled = false;
                        form.submit();                          // give up gracefully
                    });
            });
        }
    };

    // ------------------------------------------------------------
    // Inline delete confirmation
    // ------------------------------------------------------------
    // Two-step confirm in place, so deleting does not need its own page.
    function initInlineDelete() {
        document.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-confirm]');
            if (!trigger) return;
            if (trigger.getAttribute('data-confirmed') === '1') return;  // second click proceeds

            e.preventDefault();

            var original = trigger.innerHTML;
            trigger.setAttribute('data-confirmed', '1');
            trigger.classList.add('is-confirming');
            trigger.innerHTML = trigger.getAttribute('data-confirm') || 'Sure?';

            setTimeout(function () {
                trigger.removeAttribute('data-confirmed');
                trigger.classList.remove('is-confirming');
                trigger.innerHTML = original;
            }, 4000);
        });
    }

    // ------------------------------------------------------------
    // Conditional fields
    // ------------------------------------------------------------
    // <input type="checkbox" data-reveal="#target" data-reveal-when="unchecked">
    // Delegated rather than an inline <script>, because markup injected into the
    // drawer via innerHTML never executes its own scripts.
    function applyReveal(box) {
        var target = document.querySelector(box.getAttribute('data-reveal'));
        if (!target) return;
        var showWhenChecked = box.getAttribute('data-reveal-when') !== 'unchecked';
        target.hidden = showWhenChecked ? !box.checked : box.checked;
    }

    function initReveal() {
        document.addEventListener('change', function (e) {
            if (e.target.matches('[data-reveal]')) applyReveal(e.target);
        });

        // Also run whenever new markup lands (initial load and each drawer open).
        new MutationObserver(function () {
            document.querySelectorAll('[data-reveal]').forEach(applyReveal);
        }).observe(document.body, { childList: true, subtree: true });

        document.querySelectorAll('[data-reveal]').forEach(applyReveal);
    }

    // ------------------------------------------------------------
    // Grid / list view toggle, remembered per page
    // ------------------------------------------------------------
    function initViewToggle() {
        var host = document.querySelector('[data-view-host]');
        if (!host) return;

        var key = 'hms.view.' + window.location.pathname;

        function apply(mode) {
            host.setAttribute('data-view', mode);
            document.querySelectorAll('[data-view-set]').forEach(function (btn) {
                btn.classList.toggle('is-active', btn.getAttribute('data-view-set') === mode);
            });
            try { localStorage.setItem(key, mode); } catch (err) { /* ignore */ }
        }

        document.querySelectorAll('[data-view-set]').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                apply(btn.getAttribute('data-view-set'));
            });
        });

        try {
            var saved = localStorage.getItem(key);
            if (saved) apply(saved);
        } catch (err) { /* ignore */ }
    }

    document.addEventListener('DOMContentLoaded', function () {
        Drawer.init();
        initInlineDelete();
        initReveal();
        initViewToggle();
    });
})();
