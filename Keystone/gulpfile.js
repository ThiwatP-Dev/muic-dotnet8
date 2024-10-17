"use strict";
/*jshint esversion: 6 */

let gulp = require('gulp'),
    sass = require('gulp-sass'),
    cssmin = require('gulp-cssmin'),
    rename = require('gulp-rename'),
    del = require('del'),
    uglify = require('gulp-uglify'),
    rimraf = require('rimraf'),
    merge = require('merge-stream'),
    cleanCss = require('gulp-clean-css'),
    concat = require('gulp-concat'),
    uglifyES = require('gulp-uglify-es').default,
    javascriptObfuscator = require('gulp-javascript-obfuscator');

let paths = {
    scss: './scss/main.scss',
    webrootCSS: './wwwroot/css/',
};

// Dependency Dirs
var deps = {
    "animate.css": {
        "*/": ""
    },
    "bootstrap": {
        "*/**/*": ""
    },
    "bootstrap-datepicker": {
        "*/**/*": ""
    },
    "bootstrap-multiselect": {
        "*/**/*": ""
    },
    "chosen-js": {
        "*/": ""
    },
    "Chart.js": {
        "*/**": ""
    },
    "footable": {
        "*/**/*": ""
    },
    "@fortawesome/fontawesome-free": {
        "*/": ""
    },
    "fullcalendar": {
        "*/**/*": ""
    },
    "inputmask": {
        "*/**/*": ""
    },
    "jquery": {
        "*/**": ""
    },
    "jquery-ui-dist": {
        "*/": ""
    },
    "jquery.nicescroll": {
        "*/**": ""
    },
    "jquery-steps": {
        "*/**/*": ""
    },
    "jquery-validation": {
        "*/*": ""
    },
    "jquery-validation-unobtrusive": {
        "*/*": ""
    },
    "jquery.cookie": {
        "*/": ""
    },
    "moment": {
        "*/**/*": ""
    },
    "multiselect": {
        "*/**": ""
    },
    "jsbarcode": {
        "*/**": ""
    },
    "jspdf": {
        "*/**": ""
    },
    "jspdf-autotable": {
        "*/**": ""
    },
    "popper.js": {
        "*/**/*": ""
    },
    "sweetalert": {
        "*/**": ""
    },
    "prismjs": {
        "*/": "",
        "*/**": ""
    },
};

gulp.task("clean", function () {
    return del([
        './wwwroot/vendor',
        './wwwroot/js/dist',
        `${ paths.webrootCSS }main.css`,
        `${ paths.webrootCSS }main.min.css`
    ]);
});

gulp.task('scss', function () {
    gulp.src(paths.scss)
        .pipe(sass.sync().on('error', sass.logError))
        .pipe(gulp.dest(paths.webrootCSS));
});

gulp.task('minify', function () {
    return gulp.src(`${ paths.webrootCSS }main.css`)
        .pipe(cssmin())
        .pipe(rename({
            suffix: '.min'
        }))
        .pipe(gulp.dest(paths.webrootCSS));
});


gulp.task("scripts", function () {

    var streams = [];

    for (var prop in deps) {
        console.log("Prepping Scripts for: " + prop);
        for (var itemProp in deps[prop]) {
            streams.push(gulp.src("node_modules/" + prop + "/" + itemProp)
                .pipe(gulp.dest("./wwwroot/vendor/" + prop + "/" + deps[prop][itemProp])));
        }
    }

    return merge(streams);

});

gulp.task('ugly:js', function () {
    return gulp.src('./wwwroot/js/**/*.js')
        // Minify the file
        //.pipe(javascriptObfuscator())
        // Output
        .pipe(gulp.dest('./wwwroot/js/dist'));
});

gulp.task('default', ['scss', 'scripts', 'ugly:js']);