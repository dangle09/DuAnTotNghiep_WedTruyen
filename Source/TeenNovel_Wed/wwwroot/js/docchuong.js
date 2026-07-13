//--------------------------------------------------
// Dropdown chương
//--------------------------------------------------

document.querySelectorAll(".chapter-dropdown").forEach(drop => {

    const btn = drop.querySelector(".reader-btn");
    const menu = drop.querySelector(".chapter-menu");

    btn.onclick = function (e) {

        e.stopPropagation();

        menu.classList.toggle("show");
    }

});

document.onclick = function () {

    document.querySelectorAll(".chapter-menu").forEach(x => {

        x.classList.remove("show");

    });

}



//--------------------------------------------------
// Bookmark
//--------------------------------------------------

function openBookmark(line, bookmarkId, note) {

    if (bookmarkId > 0) {

        document.getElementById("infoLine").innerText = line;

        document.getElementById("infoNote").innerText = note || "";

        document.getElementById("deleteBookmarkId").value = bookmarkId;

        document.getElementById("bookmarkInfoModal").style.display = "flex";

    }
    else {

        document.getElementById("soDongInput").value = line;

        document.getElementById("bookmarkModal").style.display = "flex";

    }

}


function closeBookmark() {

    document.getElementById("bookmarkModal").style.display = "none";

}

function closeInfoBookmark() {

    document.getElementById("bookmarkInfoModal").style.display = "none";

}



// click ra ngoài để đóng

window.onclick = function (e) {

    if (e.target == document.getElementById("bookmarkModal"))
        closeBookmark();

    if (e.target == document.getElementById("bookmarkInfoModal"))
        closeInfoBookmark();

};



//--------------------------------------------------
// Scroll tới bookmark
//--------------------------------------------------

window.addEventListener("load", function () {

    const line = document.getElementById("bookmarkLine")?.value;

    if (!line) return;

    const target = document.querySelector('[data-line="' + line + '"]');

    if (!target) return;

    setTimeout(() => {

        target.scrollIntoView({
            behavior: "smooth",
            block: "center"
        });

        target.classList.add("bookmark-focus");

    }, 200);

});