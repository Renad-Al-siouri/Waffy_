const forgotForm = document.getElementById("forgotForm");
const emailInput = document.getElementById("email");
const successMsg = document.getElementById("successMsg");

if (forgotForm) {
  forgotForm.addEventListener("submit", function (e) {
    e.preventDefault();

    emailInput.classList.remove("error");
    successMsg.classList.remove("show");

    const emailValue = emailInput.value.trim();

    if (!emailValue || !emailValue.includes("@")) {
      emailInput.classList.add("error");
      return;
    }

    successMsg.classList.add("show");

    setTimeout(() => {
      window.location.href = "OTPVer.html";
    }, 700);
  });
}