window.observerInterop = {

    _bottomObserver: null,

    initializeBottom: function (dotNetHelper, element) {
        // Guard: bail out if Blazor passed a stale or unresolved reference
        if (!(element instanceof Element)) return;

        // Disconnect any previous observer before attaching a new one
        if (this._bottomObserver) {
            this._bottomObserver.disconnect();
            this._bottomObserver = null;
        }

        this._bottomObserver = new IntersectionObserver((entries) => {
            if (entries[0].isIntersecting) {
                dotNetHelper.invokeMethodAsync('TriggerLoadMore');
            }
        });

        this._bottomObserver.observe(element);
    },

    disconnectBottom: function () {
        if (this._bottomObserver) {
            this._bottomObserver.disconnect();
            this._bottomObserver = null;
        }
    },

    initializeScrollWatcher: function (dotNetHelper) {
        let lastScrollY = window.scrollY;
        let ticking = false;

        window.addEventListener('scroll', () => {
            if (!ticking) {
                window.requestAnimationFrame(() => {
                    const currentScrollY = window.scrollY;
                    const isScrollingUp = currentScrollY < lastScrollY;
                    const isAwayFromTop = currentScrollY > 50;

                    dotNetHelper.invokeMethodAsync('SetFabVisible', isScrollingUp && isAwayFromTop);

                    lastScrollY = currentScrollY;
                    ticking = false;
                });
                ticking = true;
            }
        }, { passive: true });
    }
};

window.scrollToElement = function (elementId) {
    const el = document.getElementById(elementId);
    if (!el) return;

    const scrollableParent = el.closest('.overflow-x-auto');
    if (scrollableParent) {
        const containerRect = scrollableParent.getBoundingClientRect();
        const elRect = el.getBoundingClientRect();
        const targetScrollLeft = scrollableParent.scrollLeft
            + elRect.left - containerRect.left
            - (containerRect.width / 2)
            + (elRect.width / 2);
        scrollableParent.scrollTo({ left: targetScrollLeft, behavior: 'smooth' });
    } else {
        el.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
    }
}