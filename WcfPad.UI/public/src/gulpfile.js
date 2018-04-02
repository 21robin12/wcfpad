var gulp = require('gulp');
var sass = require('gulp-sass');
var concat = require('gulp-concat');
var ts = require('gulp-typescript');

function swallowError(error) {
    console.log(error.toString());
    this.emit('end');
}

gulp.task('styles', function () {
    gulp.src('styles/scss/**/global.scss')
       .pipe(sass({
           errLogToConsole: true
       }))
       .on('error', swallowError)
       .pipe(gulp.dest('../dist/styles'));
});

gulp.task('scripts', function () {
    // Ideally I'd handle dependencies with a module loader like Browserify. I tried this, but I need 
    // requirejs(for monaco editor) and this conflicts with Browserify (requirejs overwrites require 
    // & define functions). Apparently monaco is just difficult, see here:
    // https://github.com/Microsoft/monaco-editor/issues/18#issuecomment-231788869
    // (Perhaps I could try using requirejs as my module loader? How compatible is this with TypeScript?)
    return gulp.src([
            "scripts/lib/knockout-3.4.0.min.js",
            "scripts/lib/require.js",
            "scripts/ts/**/*.ts"
        ])
        .pipe(ts({
            allowJs: true
        }))
        .pipe(concat('global.js'))
        .on('error', swallowError)
        .pipe(gulp.dest('../dist/scripts'));
});

gulp.task('watch', function () {
    gulp.watch('styles/scss/**/*.scss', ['styles']);
    gulp.watch('scripts/ts/**/*.ts', ['scripts']);
});

gulp.task('all', function() {
    return gulp.start(['styles', 'scripts']);
});

gulp.task('default', function () {
    return gulp.start(['all', 'watch']);
});