function togglePassword(button) {
  const input = document.getElementById("password");
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

document.getElementById("loginForm").addEventListener("submit", function (e) {
  e.preventDefault();

  const email = document.getElementById("email");
  const password = document.getElementById("password");
  const successMessage = document.getElementById("successMessage");
  const card = document.getElementById("loginCard");

  email.classList.remove("error");
  password.classList.remove("error");
  successMessage.classList.remove("show");
  card.classList.remove("shake");

  let valid = true;

  if (!email.value.trim() || !email.value.includes("@")) {
    email.classList.add("error");
    valid = false;
  }

  if (!password.value.trim() || password.value.length < 6) {
    password.classList.add("error");
    valid = false;
  }

  if (!valid) {
    card.classList.add("shake");
    setTimeout(() => card.classList.remove("shake"), 400);
    return;
  }

  successMessage.classList.add("show");

  setTimeout(() => {
    window.location.href = "home-after.html";
  }, 500);
});