$(function () {
    $("#nav").load("components/nav.html", function () {
        $.getScript("scripts/nav.js");
    });
});

$(function () {
    $("#home").load("components/home.html", function () {
        $.getScript("scripts/home.js");
    });
});

$(function () {
    $("#explore").load("components/explore.html", function () {
        $.getScript("scripts/explore.js");
    });
});

$(function () {
    $("#profile").load("components/profile/profile.html", function () {
        $.getScript("scripts/profile/profile.js");
    });
});

$(function () {
    $("#edit-profile").load("components/profile/edit-profile.html", function () {
        $.getScript("scripts/profile/edit-profile.js");
    });
});

$(function () {
    $("#delete-profile").load("components/profile/delete-profile.html", function () {
        $.getScript("scripts/profile/delete-profile.js");
    });
});

$(function () {
    $("#login").load("components/auth/login.html", function () {
        $.getScript("scripts/auth/login.js");
    });
});

$(function () {
    $("#register").load("components/auth/register.html", function () {
        $.getScript("scripts/auth/register.js");
    });
});

$(function () {
    $("#tweet").load("components/tweet/tweet.html", function () {
        $.getScript("scripts/tweet/tweet.js");
    });
});

$(function () {
    $("#edit-tweet").load("components/tweet/edit-tweet.html", function () {
        $.getScript("scripts/tweet/edit-tweet.js");
    });
});