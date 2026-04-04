const servicesDropdown = document.getElementById("servicesDropdown");
const servicesToggle = document.getElementById("servicesToggle");
const menuBtn = document.getElementById("menuBtn");
const navLinks = document.getElementById("navLinks");
const mobileServicesBlock = document.getElementById("mobileServicesBlock");
const mobileServicesToggle = document.getElementById("mobileServicesToggle");

const addPetBox = document.getElementById("addPetBox");
const addPetModal = document.getElementById("addPetModal");
const closeAddPetModal = document.getElementById("closeAddPetModal");
const addPetForm = document.getElementById("addPetForm");
const petsGrid = document.getElementById("petsGrid");

const petModal = document.getElementById("petModal");
const closePetModal = document.getElementById("closePetModal");
const petModalTitle = document.getElementById("petModalTitle");
const petModalBody = document.getElementById("petModalBody");

const changePhotoBtn = document.getElementById("changePhotoBtn");
const photoInput = document.getElementById("photoInput");
const avatar = document.querySelector(".avatar");

const petImageInput = document.getElementById("petImageInput");
const petImagePreview = document.getElementById("petImagePreview");

let uploadedPetImage = "";
let petCounter = 3;
let currentEditCard = null;

/* NAVBAR */
if (servicesToggle && servicesDropdown) {
    servicesToggle.addEventListener("click", function (e) {
        e.stopPropagation();
        servicesDropdown.classList.toggle("open");
    });

    document.addEventListener("click", function (e) {
        if (!servicesDropdown.contains(e.target)) {
            servicesDropdown.classList.remove("open");
        }
    });
}

if (menuBtn && navLinks) {
    menuBtn.addEventListener("click", function (e) {
        e.stopPropagation();
        navLinks.classList.toggle("open");
    });

    document.addEventListener("click", function (e) {
        if (!navLinks.contains(e.target) && !menuBtn.contains(e.target)) {
            navLinks.classList.remove("open");
        }
    });
}

if (mobileServicesToggle && mobileServicesBlock) {
    mobileServicesToggle.addEventListener("click", function (e) {
        e.stopPropagation();
        mobileServicesBlock.classList.toggle("open");
    });
}

/* CLOSE MODALS ON OUTSIDE CLICK */
window.addEventListener("click", (e) => {
    if (petModal && e.target === petModal) {
        petModal.classList.remove("show");
    }

    if (addPetModal && e.target === addPetModal) {
        addPetModal.classList.remove("show");
        resetAddPetForm();
    }
});

/* CHANGE PROFILE PHOTO */
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

/* OPEN ADD PET MODAL */
if (addPetBox && addPetModal) {
    addPetBox.addEventListener("click", () => {
        currentEditCard = null;
        resetAddPetForm();

        const title = document.querySelector("#addPetModal h2");
        const submitBtn = document.querySelector("#addPetForm .save-btn");

        if (title) title.textContent = "Add New Pet";
        if (submitBtn) submitBtn.textContent = "Add Pet";

        addPetModal.classList.add("show");
    });
}

/* CLOSE ADD PET MODAL */
if (closeAddPetModal && addPetModal) {
    closeAddPetModal.addEventListener("click", () => {
        addPetModal.classList.remove("show");
        resetAddPetForm();
    });
}

/* PET IMAGE PREVIEW IN MODAL */
if (petImageInput) {
    petImageInput.addEventListener("change", function () {
        const file = this.files[0];

        if (!file) {
            uploadedPetImage = "";
            if (petImagePreview) {
                petImagePreview.innerHTML = "No image selected";
            }
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            uploadedPetImage = e.target.result;
            if (petImagePreview) {
                petImagePreview.innerHTML = `<img src="${uploadedPetImage}" alt="Preview">`;
            }
        };
        reader.readAsDataURL(file);
    });
}

function resetAddPetForm() {
    if (!addPetForm) return;

    addPetForm.reset();
    currentEditCard = null;
    uploadedPetImage = "";

    if (petImagePreview) {
        petImagePreview.innerHTML = "No image selected";
    }
}

function createPetCard({ name, type, age, notes, petId, image }) {
    const article = document.createElement("article");
    article.className = "pet-card";
    article.setAttribute("data-pet-id", petId);

    article.innerHTML = `
    <div class="pet-image-box">
      <div class="pet-avatar-wrap">
        <div class="pet-avatar">
          <img src="${image || ""}" alt="${name}" class="pet-image" ${image ? "" : 'style="display:none;"'} />
          <span class="pet-default-icon" ${image ? 'style="display:none;"' : ""}>🐾</span>
        </div>
        <button class="pet-camera-btn" type="button">📷</button>
        <input type="file" class="pet-photo-input" accept="image/*" hidden />
      </div>
    </div>

    <div class="pet-content">
      <div class="pet-head">
        <div>
          <h3>${name}</h3>
          <p class="pet-type">${type}</p>
        </div>
        <span class="pet-dot"></span>
      </div>

      <p class="pet-age">Age: ${age} Year${Number(age) > 1 ? "s" : ""}</p>

      <ul class="pet-notes">
        <li class="good">Pet record available</li>
        ${notes ? `<li class="alert">${notes}</li>` : ""}
      </ul>

      <div class="pet-actions">
        <button class="pet-btn primary details-btn" data-pet="${petId}" type="button">View Details</button>
        <button class="pet-btn secondary edit-pet-btn" type="button">Edit</button>
        <button class="pet-btn danger delete-pet-btn" type="button">Delete</button>
      </div>
    </div>
  `;

    return article;
}

/* ADD OR EDIT PET */
if (addPetForm) {
    addPetForm.addEventListener("submit", (e) => {
        e.preventDefault();

        const name = document.getElementById("petNameInput").value.trim();
        const type = document.getElementById("petTypeInput").value;
        const breed = document.getElementById("petBreedInput").value.trim();
        const age = document.getElementById("petAgeInput").value.trim();
        const gender = document.getElementById("petGenderInput").value;
        const color = document.getElementById("petColorInput").value.trim();

        const health = document.getElementById("petHealthInput").value;
        const vaccinated = document.getElementById("petVaccinatedInput").value;
        const vaccDate = document.getElementById("petVaccDateInput").value;
        const neuter = document.getElementById("petNeuterInput").value;

        const notes = document.getElementById("petNotesInput").value.trim();
        const energy = document.getElementById("petEnergyInput").value;
        const children = document.getElementById("petChildrenInput").value;
        const otherPets = document.getElementById("petOtherPetsInput").value;
        const desc = document.getElementById("petDescInput").value.trim();

        if (!name || !type || !age) {
            alert("Please fill in the required fields.");
            return;
        }

        if (currentEditCard) {
            currentEditCard.querySelector("h3").textContent = name;
            currentEditCard.querySelector(".pet-type").textContent = type;
            currentEditCard.querySelector(".pet-age").textContent =
                `Age: ${age} Year${Number(age) > 1 ? "s" : ""}`;

            const notesList = currentEditCard.querySelector(".pet-notes");
            notesList.innerHTML = `
        <li class="good">${health || "Pet record available"}</li>
        ${notes ? `<li class="alert">${notes}</li>` : ""}
      `;

            const imageBox = currentEditCard.querySelector(".pet-image-box");

            if (uploadedPetImage) {
                imageBox.innerHTML = `
          <div class="pet-avatar-wrap">
            <div class="pet-avatar">
              <img src="${uploadedPetImage}" alt="${name}" class="pet-image" />
              <span class="pet-default-icon" style="display: none;">🐾</span>
            </div>
            <button class="pet-camera-btn" type="button">📷</button>
            <input type="file" class="pet-photo-input" accept="image/*" hidden />
          </div>
        `;
            }

            addPetModal.classList.remove("show");
            resetAddPetForm();
            alert("Pet updated successfully!");
            return;
        }

        const petId = `pet${petCounter++}`;

        const newPetCard = createPetCard({
            name,
            type,
            age,
            notes: notes || health,
            petId,
            image: uploadedPetImage
        });

        petsGrid.appendChild(newPetCard);

        addPetModal.classList.remove("show");
        resetAddPetForm();
        alert("Pet added successfully!");
    });
}

/* PET BUTTONS - EVENT DELEGATION */
if (petsGrid) {
    petsGrid.addEventListener("click", (e) => {
        const detailsBtn = e.target.closest(".details-btn");
        const deleteBtn = e.target.closest(".delete-pet-btn");
        const editBtn = e.target.closest(".edit-pet-btn");

        if (detailsBtn) {
            const petKey = detailsBtn.dataset.pet || "buddy";
            window.location.href = `petRecord.html?pet=${petKey}`;
            return;
        }

        if (deleteBtn) {
            const petCard = deleteBtn.closest(".pet-card");
            if (!petCard) return;

            const petName = petCard.querySelector("h3").textContent;
            if (confirm(`Delete ${petName}?`)) {
                petCard.remove();
                alert(`${petName} deleted.`);
            }
            return;
        }

        if (editBtn) {
            const petCard = editBtn.closest(".pet-card");
            if (!petCard) return;

            currentEditCard = petCard;

            const petName = petCard.querySelector("h3").textContent;
            const petType = petCard.querySelector(".pet-type").textContent;
            const petAgeText = petCard.querySelector(".pet-age").textContent;
            const petAge = petAgeText.replace("Age:", "").replace("Years", "").replace("Year", "").trim();

            const noteItems = petCard.querySelectorAll(".pet-notes li");
            let customNote = "";
            if (noteItems.length > 1) {
                customNote = noteItems[1].textContent.trim();
            }

            document.getElementById("petNameInput").value = petName;
            document.getElementById("petTypeInput").value = petType;
            document.getElementById("petAgeInput").value = petAge;
            document.getElementById("petNotesInput").value = customNote;

            const currentImage = petCard.querySelector(".pet-image");
            if (currentImage && currentImage.src) {
                uploadedPetImage = currentImage.src;
                if (petImagePreview) {
                    petImagePreview.innerHTML = `<img src="${uploadedPetImage}" alt="Preview">`;
                }
            } else {
                uploadedPetImage = "";
                if (petImagePreview) {
                    petImagePreview.innerHTML = "No image selected";
                }
            }

            const title = document.querySelector("#addPetModal h2");
            const submitBtn = document.querySelector("#addPetForm .save-btn");

            if (title) title.textContent = "Edit Pet";
            if (submitBtn) submitBtn.textContent = "Save Changes";

            addPetModal.classList.add("show");
            return;
        }
    });

    document.addEventListener("click", (e) => {
        const cameraBtn = e.target.closest(".pet-camera-btn");
        if (!cameraBtn) return;

        const petCard = cameraBtn.closest(".pet-card");
        if (!petCard) return;

        const fileInput = petCard.querySelector(".pet-photo-input");
        if (fileInput) fileInput.click();
    });

    document.addEventListener("change", (e) => {
        if (!e.target.classList.contains("pet-photo-input")) return;

        const file = e.target.files[0];
        if (!file) return;

        const petCard = e.target.closest(".pet-card");
        if (!petCard) return;

        const petImage = petCard.querySelector(".pet-image");
        const fallbackIcon = petCard.querySelector(".pet-default-icon");

        const reader = new FileReader();
        reader.onload = function (ev) {
            petImage.src = ev.target.result;
            petImage.style.display = "block";
            if (fallbackIcon) fallbackIcon.style.display = "none";
        };
        reader.readAsDataURL(file);
    });
}

/* PET DETAILS MODAL CLOSE */
if (closePetModal && petModal) {
    closePetModal.addEventListener("click", () => {
        petModal.classList.remove("show");
    });
}