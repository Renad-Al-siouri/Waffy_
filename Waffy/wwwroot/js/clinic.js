function byId(id) {
    return document.getElementById(id);
}

const CW_TOTAL_STEPS = 2;

function cwSetProgress(step) {
    const fill = document.querySelector(".prog-fill");
    const counter = byId("cwClinicCounter");
    const indicators = document.querySelectorAll("#cwClinicIndicators .cw-ind");

    if (fill) {
        fill.style.width = `${(step / CW_TOTAL_STEPS) * 100}%`;
    }

    if (counter) {
        counter.textContent = `Step ${step} of ${CW_TOTAL_STEPS}`;
    }

    indicators.forEach((ind, index) => {
        const n = index + 1;
        const dot = ind.querySelector(".cw-dot");

        ind.classList.remove("active", "done");

        if (n < step) {
            ind.classList.add("done");
            if (dot) dot.textContent = "✓";
        } else if (n === step) {
            ind.classList.add("active");
            if (dot) dot.textContent = n;
        } else {
            if (dot) dot.textContent = n;
        }
    });
}

function cwGoStep2() {
    const clinicName = byId("cc-clinicName");
    const city = byId("cc-city");
    const phone = byId("cc-phone");

    if (!clinicName || !clinicName.value.trim()) {
        alert("Please enter clinic name.");
        clinicName?.focus();
        return;
    }

    if (!city || !city.value.trim()) {
        alert("Please select city.");
        city?.focus();
        return;
    }

    if (!phone || !phone.value.trim()) {
        alert("Please enter clinic phone number.");
        phone?.focus();
        return;
    }

    const municipal = byId("cw-doc-municipal");
    const vet = byId("cw-doc-vet");
    const idDoc = byId("cw-doc-id");

    if (!municipal || !municipal.files.length) {
        alert("Please upload Municipal License.");
        return;
    }

    if (!vet || !vet.files.length) {
        alert("Please upload Veterinary Practice License.");
        return;
    }

    if (!idDoc || !idDoc.files.length) {
        alert("Please upload Clinic Manager National ID.");
        return;
    }

    sessionStorage.setItem("cwReviewName", clinicName.value);
    sessionStorage.setItem("cwReviewCity", city.value);
    sessionStorage.setItem("cwReviewPhone", phone.value);

    window.location.href = "/Account/Clinic_step2";
}

function cwSubmitClinicTwoSteps() {
    const fullName = byId("co-fullName");
    const phone = byId("co-phone");
    const email = byId("co-email");
    const password = byId("co-password");
    const confirmPassword = byId("co-confirmPassword");

    if (!fullName || !fullName.value.trim()) {
        alert("Please enter full name.");
        fullName?.focus();
        return;
    }

    if (!phone || !phone.value.trim()) {
        alert("Please enter phone number.");
        phone?.focus();
        return;
    }

    if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value)) {
        alert("Please enter a valid email address.");
        email?.focus();
        return;
    }

    if (!password || password.value.length < 8) {
        alert("Password must be at least 8 characters.");
        password?.focus();
        return;
    }

    if (!confirmPassword || password.value !== confirmPassword.value) {
        alert("Passwords do not match.");
        confirmPassword?.focus();
        return;
    }

    const form = byId("cwClinicForm");
    const success = byId("cwSuccess");

    if (form) form.style.display = "none";
    if (success) success.style.display = "block";

    window.scrollTo({ top: 0, behavior: "smooth" });
}

function cwTrigUpload(id) {
    const input = byId(id);
    if (input) input.click();
}

function cwPreviewFile(input, previewId, nameId) {
    if (!input || !input.files || !input.files[0]) return;

    const preview = byId(previewId);
    const name = byId(nameId);

    if (name) name.textContent = input.files[0].name;
    if (preview) preview.classList.add("show");
}

function cwRemoveFile(fileId, previewId, nameId) {
    const fileInput = byId(fileId);
    const preview = byId(previewId);
    const name = byId(nameId);

    if (fileInput) fileInput.value = "";
    if (preview) preview.classList.remove("show");
    if (name) name.textContent = "";
}

document.addEventListener("DOMContentLoaded", () => {
    if (document.body.classList.contains("clinic-step-1")) {
        cwSetProgress(1);
    }

    if (document.body.classList.contains("clinic-step-2")) {
        cwSetProgress(2);

        const reviewName = sessionStorage.getItem("cwReviewName");
        const reviewCity = sessionStorage.getItem("cwReviewCity");
        const reviewPhone = sessionStorage.getItem("cwReviewPhone");

        if (byId("cwReviewName") && reviewName) byId("cwReviewName").textContent = reviewName;
        if (byId("cwReviewCity") && reviewCity) byId("cwReviewCity").textContent = reviewCity;
        if (byId("cwReviewPhone") && reviewPhone) byId("cwReviewPhone").textContent = reviewPhone;
    }
});