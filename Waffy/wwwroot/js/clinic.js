function byId(id) {
    return document.getElementById(id);
}

let cwClinicStep = 1;
let cwDoctorStep = 1;
let cwSelectedPlan = "basic";

const CW_CLINIC_TOTAL = 4;
const CW_DOCTOR_TOTAL = 4;

function cwUpdateSidePanel(type) {
    const list = byId("cwStepsList");
    if (!list) return;

    if (type === "initial") {
        list.innerHTML = `
      <div class="cw-step-item">
        <div class="cw-step-num active">1</div>
        <div class="cw-step-text">Choose Account Type</div>
      </div>
    `;
    } else if (type === "clinic") {
        const steps = [
            "Clinic Information",
            "Schedule & Plan",
            "Official Documents",
            "Manager Info"
        ];

        list.innerHTML = steps
            .map(
                (s, i) => `
      <div class="cw-step-item">
        <div class="cw-step-num ${i === 0 ? "active" : ""}" id="cwcs${i + 1}">
          ${i + 1}
        </div>
        <div class="cw-step-text">${s}</div>
      </div>
    `
            )
            .join("");
    } else if (type === "doctor") {
        const steps = [
            "Invite Code",
            "Personal Info",
            "Specialty & Schedule",
            "Official Documents"
        ];

        list.innerHTML = steps
            .map(
                (s, i) => `
      <div class="cw-step-item">
        <div class="cw-step-num ${i === 0 ? "active" : ""}" id="cwds${i + 1}">
          ${i + 1}
        </div>
        <div class="cw-step-text">${s}</div>
      </div>
    `
            )
            .join("");
    }
}

function cwUpdateSidePanelStep(prefix, current, total) {
    for (let i = 1; i <= total; i++) {
        const el = byId(`${prefix}${i}`);
        if (!el) continue;

        el.classList.remove("active", "done");

        if (i < current) {
            el.classList.add("done");
            el.textContent = "✓";
        } else if (i === current) {
            el.classList.add("active");
            el.textContent = i;
        } else {
            el.textContent = i;
        }
    }
}

function cwUpdateIndicators(containerId, current, total) {
    const inds = document.querySelectorAll(`#${containerId} .cw-ind`);

    inds.forEach((ind, i) => {
        const n = i + 1;
        const dot = ind.querySelector(".cw-dot");
        if (!dot) return;

        ind.classList.remove("active", "done");

        if (n < current) {
            ind.classList.add("done");
            dot.textContent = "✓";
        } else if (n === current) {
            ind.classList.add("active");
            dot.textContent = n;
        } else {
            dot.textContent = n;
        }
    });
}

function cwSelectSubRole(type, el) {
    document.querySelectorAll("#clinicWizardContainer .cw-sub-role-card").forEach(c => {
        c.classList.remove("selected");
    });

    if (el) el.classList.add("selected");

    setTimeout(() => {
        const roleSelector = byId("cwRoleSelector");
        const clinicForm = byId("cwClinicForm");
        const doctorForm = byId("cwDoctorForm");

        if (roleSelector) roleSelector.style.display = "none";

        if (type === "clinic-owner") {
            if (clinicForm) clinicForm.classList.add("show");
            if (doctorForm) doctorForm.classList.remove("show");

            cwClinicStep = 1;
            cwUpdateIndicators("cwClinicIndicators", 1, CW_CLINIC_TOTAL);

            if (byId("cwClinicCounter")) {
                byId("cwClinicCounter").textContent = "Step 1 of 4";
            }

            document.querySelectorAll('[id^="cc-"]').forEach(s => {
                if (s.id === "cc-1") s.classList.remove("hidden");
                else s.classList.add("hidden");
            });

            cwUpdateSidePanel("clinic");
        } else {
            if (doctorForm) doctorForm.classList.add("show");
            if (clinicForm) clinicForm.classList.remove("show");

            cwDoctorStep = 1;
            cwUpdateIndicators("cwDoctorIndicators", 1, CW_DOCTOR_TOTAL);

            if (byId("cwDoctorCounter")) {
                byId("cwDoctorCounter").textContent = "Step 1 of 4";
            }

            document.querySelectorAll('[id^="cd-"]').forEach(s => {
                if (s.id === "cd-1") s.classList.remove("hidden");
                else s.classList.add("hidden");
            });

            cwUpdateSidePanel("doctor");
        }

        window.scrollTo({ top: 0, behavior: "smooth" });
    }, 300);
}

function cwGoRoleSelector() {
    const roleSelector = byId("cwRoleSelector");
    const clinicForm = byId("cwClinicForm");
    const doctorForm = byId("cwDoctorForm");
    const success = byId("cwSuccess");

    if (roleSelector) roleSelector.style.display = "block";
    if (clinicForm) clinicForm.classList.remove("show");
    if (doctorForm) doctorForm.classList.remove("show");
    if (success) success.classList.remove("show");

    document.querySelectorAll("#clinicWizardContainer .cw-sub-role-card").forEach(c => {
        c.classList.remove("selected");
    });

    cwUpdateSidePanel("initial");
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function cwClinicNext(from) {
    if (from === 3) {
        const n = byId("cc-clinicName");
        const c = byId("cc-city");
        const p = byId("cc-phone");

        if (byId("cwReviewName")) {
            byId("cwReviewName").textContent = n && n.value ? n.value : "—";
        }

        if (byId("cwReviewCity")) {
            byId("cwReviewCity").textContent = c && c.value ? c.value : "—";
        }

        if (byId("cwReviewPhone")) {
            byId("cwReviewPhone").textContent = p && p.value ? p.value : "—";
        }
    }

    cwClinicGo(from, from + 1);
}

function cwClinicGo(from, to) {
    const fromEl = byId(`cc-${from}`);
    const toEl = byId(`cc-${to}`);

    if (fromEl) fromEl.classList.add("hidden");
    if (toEl) toEl.classList.remove("hidden");

    cwClinicStep = to;
    cwUpdateIndicators("cwClinicIndicators", to, CW_CLINIC_TOTAL);

    if (byId("cwClinicCounter")) {
        byId("cwClinicCounter").textContent = `Step ${to} of ${CW_CLINIC_TOTAL}`;
    }

    cwUpdateSidePanelStep("cwcs", to, CW_CLINIC_TOTAL);

    const progFill = document.querySelector(".prog-fill");
    if (progFill) {
        progFill.style.width = `${(to / CW_CLINIC_TOTAL) * 100}%`;
    }

    window.scrollTo({ top: 60, behavior: "smooth" });
}

function cwSubmitClinic() {
    const consent = byId("cwClinicConsent");

    if (!consent || !consent.checked) {
        if (consent) {
            consent.style.outline = "2px solid red";
            setTimeout(() => {
                consent.style.outline = "";
            }, 1200);
        }
        return;
    }

    const clinicForm = byId("cwClinicForm");
    const success = byId("cwSuccess");

    if (clinicForm) clinicForm.classList.remove("show");
    if (success) success.classList.add("show");

    if (byId("cwSuccessIcon")) byId("cwSuccessIcon").textContent = "🏥";
    if (byId("cwSuccessTitle")) byId("cwSuccessTitle").textContent = "Clinic Application Submitted!";
    if (byId("cwSuccessSub")) {
        byId("cwSuccessSub").textContent =
            "The Waffy team will review your application and approve it within 24 hours.";
    }

    window.scrollTo({ top: 0, behavior: "smooth" });
}

function cwDoctorNext(from) {
    if (from === 1) {
        const inv = byId("cwInviteInput");
        const suc = byId("cwInviteSuccess");

        if (suc && !suc.classList.contains("show") && (!inv || inv.value.trim().length < 6)) {
            if (inv) inv.classList.add("error");
            return;
        }

        const code = byId("cwInviteInput");
        if (byId("cwReviewCode") && code) {
            byId("cwReviewCode").textContent = code.value.toUpperCase();
        }
    }

    cwDoctorGo(from, from + 1);
}

function cwDoctorGo(from, to) {
    const fromEl = byId(`cd-${from}`);
    const toEl = byId(`cd-${to}`);

    if (fromEl) fromEl.classList.add("hidden");
    if (toEl) toEl.classList.remove("hidden");

    cwDoctorStep = to;
    cwUpdateIndicators("cwDoctorIndicators", to, CW_DOCTOR_TOTAL);

    if (byId("cwDoctorCounter")) {
        byId("cwDoctorCounter").textContent = `Step ${to} of ${CW_DOCTOR_TOTAL}`;
    }

    cwUpdateSidePanelStep("cwds", to, CW_DOCTOR_TOTAL);

    window.scrollTo({ top: 60, behavior: "smooth" });
}

function cwSubmitDoctor() {
    const consent = byId("cwDoctorConsent");

    if (!consent || !consent.checked) {
        if (consent) {
            consent.style.outline = "2px solid red";
            setTimeout(() => {
                consent.style.outline = "";
            }, 1200);
        }
        return;
    }

    const doctorForm = byId("cwDoctorForm");
    const success = byId("cwSuccess");

    if (doctorForm) doctorForm.classList.remove("show");
    if (success) success.classList.add("show");

    if (byId("cwSuccessIcon")) byId("cwSuccessIcon").textContent = "🩺";
    if (byId("cwSuccessTitle")) byId("cwSuccessTitle").textContent = "Doctor Profile Submitted!";
    if (byId("cwSuccessSub")) {
        byId("cwSuccessSub").textContent =
            "Your profile has been sent to the clinic admin for approval and activation.";
    }

    window.scrollTo({ top: 0, behavior: "smooth" });
}

function cwSelectPlan(el, plan) {
    document.querySelectorAll("#clinicWizardContainer .cw-plan").forEach(c => {
        c.classList.remove("active");
    });

    if (el) el.classList.add("active");
    cwSelectedPlan = plan;

    const payment = byId("cwPaymentDetails");
    if (payment) {
        payment.style.display = plan === "free" ? "none" : "block";
    }

    if (byId("cwReviewPlan")) {
        const labels = {
            free: "Free — 0 JOD/month",
            basic: "Basic — 25 JOD/month",
            pro: "Pro — 60 JOD/month"
        };

        byId("cwReviewPlan").textContent = labels[plan] || "—";
    }
}

function cwTrigUpload(id) {
    const el = byId(id);
    if (el) el.click();
}

function cwPreviewFile(inp, prevId, nameId) {
    if (!inp || !inp.files || !inp.files[0]) return;

    const nameEl = byId(nameId);
    const prevEl = byId(prevId);

    if (nameEl) nameEl.textContent = inp.files[0].name;
    if (prevEl) prevEl.classList.add("show");
}

function cwRemoveFile(fileId, prevId, nameId) {
    const f = byId(fileId);
    const p = byId(prevId);
    const n = byId(nameId);

    if (f) f.value = "";
    if (n) n.textContent = "";
    if (p) p.classList.remove("show");
}

function cwAddImg(thumb) {
    const inp = document.createElement("input");
    inp.type = "file";
    inp.accept = "image/*";
    inp.style.display = "none";

    document.body.appendChild(inp);
    inp.click();

    inp.onchange = () => {
        if (!inp.files || !inp.files[0]) {
            document.body.removeChild(inp);
            return;
        }

        const reader = new FileReader();

        reader.onload = e => {
            const img = document.createElement("img");
            img.src = e.target.result;

            thumb.innerHTML = "";
            thumb.appendChild(img);

            const rm = document.createElement("button");
            rm.className = "cw-thumb-rm";
            rm.textContent = "✕";
            rm.type = "button";

            rm.onclick = ev => {
                ev.stopPropagation();
                thumb.innerHTML = "➕";
                thumb.classList.remove("filled");
                thumb.onclick = () => cwAddImg(thumb);
            };

            thumb.appendChild(rm);
            thumb.classList.add("filled");
            thumb.onclick = null;
        };

        reader.readAsDataURL(inp.files[0]);
        document.body.removeChild(inp);
    };
}

function cwVerifyCode() {
    const input = byId("cwInviteInput");
    const success = byId("cwInviteSuccess");

    if (!input || !success) return;

    const value = input.value.trim();
    if (value.length < 6) {
        input.classList.add("error");
        success.classList.remove("show");
        return;
    }

    input.classList.remove("error");
    success.classList.add("show");
}

document.addEventListener("DOMContentLoaded", () => {
    if (byId("cwStepsList")) {
        cwUpdateSidePanel("clinic");
    }

    const defPlan = document.querySelector("#clinicWizardContainer .cw-plan.active");
    if (defPlan) {
        cwSelectPlan(defPlan, "basic");
    } else {
        const payment = byId("cwPaymentDetails");
        if (payment) payment.style.display = "block";
    }

    const inviteInput = byId("cwInviteInput");
    if (inviteInput) {
        inviteInput.addEventListener("keyup", e => {
            if (e.key === "Enter") cwVerifyCode();
            e.target.value = e.target.value.toUpperCase().replace(/[^A-Z0-9\-]/g, "");
        });
    }
});