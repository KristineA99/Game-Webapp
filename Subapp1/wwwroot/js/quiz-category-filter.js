document.addEventListener("DOMContentLoaded", () => {
    const categoryCheckboxes =
        document.querySelectorAll(".category-checkbox");

    const questionItems =
        document.querySelectorAll(".question-item");

    const questionHelp =
        document.getElementById("question-help");

    function updateQuestionVisibility() {
        const selectedCategories = new Set(
            Array.from(categoryCheckboxes)
                .filter(checkbox => checkbox.checked)
                .map(checkbox => checkbox.value)
        );

        let visibleCount = 0;

        questionItems.forEach(item => {
            const categoryId = item.dataset.categoryId;
            const checkbox = item.querySelector(".question-checkbox");

            const isVisible = selectedCategories.has(categoryId);

            item.hidden = !isVisible;
            checkbox.disabled = !isVisible;

            // Clear questions when their category is deselected.
            if (!isVisible) {
                checkbox.checked = false;
            }

            if (isVisible) {
                visibleCount++;
            }
        });

        questionHelp.textContent = selectedCategories.size === 0
            ? "Select a category above to display its questions."
            : `${visibleCount} questions available. Select the questions to include.`;
    }

    categoryCheckboxes.forEach(checkbox => {
        checkbox.addEventListener("change", updateQuestionVisibility);
    });

    // Initialize visibility when the page loads.
    updateQuestionVisibility();
});