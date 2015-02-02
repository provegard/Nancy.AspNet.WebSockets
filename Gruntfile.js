var relSolutionPath = 'src/Nancy.AspNet.WebSockets.sln',
    solutionConfig = 'Debug';

module.exports = function (grunt) {

    grunt.initConfig({

        msbuild: {
            options: {
                maxCpuCount: 4,
                buildParameters: {
                    WarningLevel: 2
                },
                verbosity: 'quiet',
                projectConfiguration: [solutionConfig],
            },
            clean: {
                src: [relSolutionPath],
                options: {
                    targets: ['Clean'],
                }
            },
            dev: {
                src: [relSolutionPath],
                options: {
                    targets: ['Build'],
                }
            }
        },

        nugetrestore: {
            restore: {
                src: 'src/**/packages.config',
                dest: 'src/packages'
            }
        },

        exec: {
            jstest: {
                command: 'src\\packages\\Chutzpah.3.3.1\\tools\\chutzpah.console.exe src\\Nancy.AspNet.WebSockets.Sample.Tests\\JavaScript'
            }
        },

        nunit: {
            test: {
                files: {
                    src: [relSolutionPath]
                }
            },
            options: {
                path: 'src/packages/NUnit.Runners.2.6.4/tools',
                config: solutionConfig,
                noshadow: true,
                labels: true
            }
        }

    });

    grunt.loadNpmTasks('grunt-nuget');
    grunt.loadNpmTasks('grunt-msbuild');
    grunt.loadNpmTasks('grunt-exec');
    grunt.loadNpmTasks('grunt-nunit-runner');

    grunt.registerTask('clean', ['msbuild:clean']);
    grunt.registerTask('package-restore', ['nugetrestore:restore']);
    grunt.registerTask('compile', ['clean', 'package-restore', 'msbuild:dev']);
    grunt.registerTask('test', ['compile', 'exec:jstest', 'nunit:test']);
    grunt.registerTask('default', ['test']);

};
