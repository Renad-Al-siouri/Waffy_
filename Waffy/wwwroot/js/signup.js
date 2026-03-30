function goToPage(page) {
  window.location.href = page;
}

document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".social-btn").forEach((btn) => {
    btn.addEventListener("click", function () {
      const platform = this.classList.contains("google")
        ? "Google"
        : this.classList.contains("facebook")
        ? "Facebook"
        : "Apple";

      alert("Connecting to " + platform + " ... 🚀");
    });
  });
});