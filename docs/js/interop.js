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
    if (!el) return;

    // For pill buttons: manually scroll the overflow container instead of using
    // scrollIntoView(), which triggers Safari's focus management and steals focus
    // from the clicked button to the adjacent one.
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