window.observerInterop = {
    initializeBottom: function (dotNetHelper, element) {
        let observer = new IntersectionObserver((entries) => {
            if (entries[0].isIntersecting) {
                dotNetHelper.invokeMethodAsync('TriggerLoadMore');
            }
        });
        observer.observe(element);
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
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
    }
}