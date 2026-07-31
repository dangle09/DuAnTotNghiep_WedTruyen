//layout-docgia.js

// Mobile menu toggle
const mobileToggle = document.getElementById('mobileMenuToggle');
const mobileMenu   = document.getElementById('mobileMenu');

if (mobileToggle && mobileMenu) {
    mobileToggle.addEventListener('click', () => {
        mobileMenu.classList.toggle('open');
        const icon = mobileToggle.querySelector('i');
        if (icon) {
            icon.className = mobileMenu.classList.contains('open')
                ? 'bi bi-x-lg'
                : 'bi bi-list';
        }
    });
}

// Tự ẩn toast sau 4 giây
document.querySelectorAll('.ih-toast').forEach(toast => {
    setTimeout(() => {
        toast.style.transition = 'opacity .3s, transform .3s';
        toast.style.opacity    = '0';
        toast.style.transform  = 'translateY(-6px)';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
});

// Active link: tự highlight nav theo URL hiện tại
const currentPath = window.location.pathname.toLowerCase();
document.querySelectorAll('.ih-nav-links a, .ih-mobile-menu a').forEach(link => {
    const href = link.getAttribute('href')?.toLowerCase();
    if (href && currentPath.startsWith(href) && href !== '/') {
        link.classList.add('active');
    }
    if (href === '/' && currentPath === '/') {
        link.classList.add('active');
    }
});
