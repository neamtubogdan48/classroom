document.addEventListener("DOMContentLoaded", function () {
    const footer = document.querySelector("footer");

    function checkFooterVisibility() {
        const scrollTop = window.scrollY || document.documentElement.scrollTop;
        const windowHeight = window.innerHeight;
        const documentHeight = document.documentElement.scrollHeight;

        // Show the footer only when the user is at the bottom of the page
        if (scrollTop + windowHeight >= documentHeight) {
            footer.style.display = "none";
        } else {
            footer.style.display = "none";
        }
    }

    // Initial check
    checkFooterVisibility();

    // Check on scroll
    window.addEventListener("scroll", checkFooterVisibility);

    // Check on resize
    window.addEventListener("resize", checkFooterVisibility);
});
