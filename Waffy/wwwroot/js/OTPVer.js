const otpInputs = document.querySelectorAll(".otp-input");
const otpForm = document.getElementById("otpForm");
const resendCode = document.getElementById("resendCode");

let countdown = 30;
let timer;

startTimer();

function startTimer() {
    clearInterval(timer);
    resendCode.classList.add("disabled");
    resendCode.textContent = `Resend in ${countdown}s`;

    timer = setInterval(() => {
        countdown--;

        if (countdown >= 0) {
            resendCode.textContent = `Resend in ${countdown}s`;
        }

        if (countdown < 0) {
            clearInterval(timer);
            resendCode.textContent = "Resend Code";
            resendCode.classList.remove("disabled");
        }
    }, 1000);
}

otpInputs.forEach((input, index) => {
    input.addEventListener("input", () => {
        input.value = input.value.replace(/[^0-9]/g, "");

        if (input.value && index < otpInputs.length - 1) {
            otpInputs[index + 1].focus();
        }
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Backspace" && !input.value && index > 0) {
            otpInputs[index - 1].focus();
        }
    });

    input.addEventListener("paste", (e) => {
        e.preventDefault();

        const pasted = (e.clipboardData || window.clipboardData)
            .getData("text")
            .replace(/\D/g, "")
            .slice(0, 4);

        pasted.split("").forEach((char, i) => {
            if (otpInputs[i]) otpInputs[i].value = char;
        });

        if (pasted.length) {
            otpInputs[Math.min(pasted.length - 1, 3)].focus();
        }
    });
});

otpForm.addEventListener("submit", function (e) {
    let valid = true;
    let code = "";

    otpInputs.forEach((input) => {
        if (!input.value.trim()) {
            valid = false;
        }

        code += input.value.trim();
    });

    if (!valid || code.length < 4) {
        e.preventDefault();
        return;
    }

    document.getElementById("finalCode").value = code;
});

resendCode.addEventListener("click", function (e) {
    e.preventDefault();

    if (resendCode.classList.contains("disabled")) return;

    alert("A new code has been sent.");
    countdown = 30;
    startTimer();
});