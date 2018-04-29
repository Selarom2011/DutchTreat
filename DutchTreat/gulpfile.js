var gulp = require('gulp');
var uglify = require('gulp-uglify');
var concat = require('gulp-concat');
var rimraf = require("rimraf");
var merge = require('merge-stream');

gulp.task("minify", function () {

    var streams = [
        gulp.src(["wwwroot/js/*.js"])
            .pipe(uglify())
            .pipe(concat("index.min.js"))
            .pipe(gulp.dest("wwwroot/lib/site"))
    ];

    return merge(streams);
});

// Dependency Dirs
var deps = {
    "jquery": {
        "dist/*": ""
    },
    "font-awesome": {
        "/**/*": ""
    },
    "bootstrap": {
        "dist/**/*": ""
    },
    "jquery-validation": {
        "dist/*": ""
    },
    "jquery-validation-unobtrusive": {
        "/*": ""
    }
};

gulp.task("clean", function (cb) {
    return rimraf("wwwroot/vendor/", cb);
});

gulp.task("scripts", function () {

    var streams = [];

    for (var prop in deps) {
        console.log("Prepping Scripts for: " + prop);
        for (var itemProp in deps[prop]) {
            console.log("Prepping for: " + itemProp)
            streams.push(gulp.src("node_modules/" + prop + "/" + itemProp)
                .pipe(gulp.dest("wwwroot/vendor/" + prop + "/" + deps[prop][itemProp])));
        }
    }

    return merge(streams);

});

gulp.task("default", ['clean', 'minify', 'scripts']);