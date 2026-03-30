function togglePassword(inputId, button) {
  const input = document.getElementById(inputId);
  const icon = button.querySelector("i");

  if (input.type === "password") {
    input.type = "text";
    icon.classList.remove("fa-eye");
    icon.classList.add("fa-eye-slash");
  } else {
    input.type = "password";
    icon.classList.remove("fa-eye-slash");
    icon.classList.add("fa-eye");
  }
}

document.getElementById("resetForm").addEventListener("submit", function (e) {
  e.preventDefault();

  const newPassword = document.getElementById("newPassword");
  const confirmPassword = document.getElementById("confirmPassword");
  const resetView = document.getElementById("resetView");
  const successView = document.getElementById("successView");

  newPassword.classList.remove("error");
  confirmPassword.classList.remove("error");

  let valid = true;

  if (!newPassword.value.trim() || newPassword.value.length < 6) {
    newPassword.classList.add("error");
    valid = false;
  }

  if (!confirmPassword.value.trim() || confirmPassword.value !== newPassword.value) {
    confirmPassword.classList.add("error");
    valid = false;
  }

  if (!valid) return;

  resetView.classList.add("hide");
  successView.classList.add("show");
});