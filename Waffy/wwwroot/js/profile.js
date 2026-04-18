// ══════════════════════════════════════
// NAVBAR
// ══════════════════════════════════════
const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

if (servicesToggle && servicesDropdown) {
    servicesToggle.addEventListener("click", function (e) {
        e.stopPropagation();
        servicesDropdown.classList.toggle("open");
    });
    document.addEventListener("click", function (e) {
        if (!servicesDropdown.contains(e.target))
            servicesDropdown.classList.remove("open");
    });
}

if (menuBtn && navLinks) {
    menuBtn.addEventListener("click", function (e) {
        e.stopPropagation();
        navLinks.classList.toggle("open");
    });
    document.addEventListener("click", function (e) {
        if (!navLinks.contains(e.target) && e.target !== menuBtn)
            navLinks.classList.remove("open");
    });
}

if (mobileServicesToggle && mobileServicesBlock) {
    mobileServicesToggle.addEventListener("click", function (e) {
        e.stopPropagation();
        mobileServicesBlock.classList.toggle("open");
    });
}

// ══════════════════════════════════════
// CHANGE PROFILE PHOTO — يعرض Preview فقط
// الحفظ الحقيقي عبر Edit_Profile form
// ══════════════════════════════════════
const changePhotoBtn = document.getElementById("changePhotoBtn");
const photoInput = document.getElementById("photoInput");
const avatar = document.querySelector(".avatar");

if (changePhotoBtn && photoInput && avatar) {
    changePhotoBtn.addEventListener("click", () => {
        photoInput.click();
    });

    photoInput.addEventListener("change", function () {
        const file = this.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (e) {
            avatar.innerHTML = `<img src="${e.target.result}" alt="Profile Photo">`;
        };
        reader.readAsDataURL(file);
    });
}