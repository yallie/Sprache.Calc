#This script will fetch semver-compatible information from git tag
#it should be launched from root folder of repository as working directory
#script expects a git tag in <major>.<minor>.<patch>-<stage> format
#where stage part is optional
#script will provide version information in form of bash variables
#for .Net assemblies versioning (https://andrewlock.net/version-vs-versionsuffix-vs-packageversion-what-do-they-all-mean/):
#${fileVersion}
#${informationVersion}
#${version}
#if tag is found on HEAD, version will be taken as-is from it
#if we have some commits on top of latest tag script will bump
#the patch part from this tag by one, and, if stage is missing, add '-preview' stage


#if we are not on commit with tag, lets put information about number of commits from latest tag
describe=`git describe --tags --always`
#fallback if we have no tags at all reachable from HEAD
#tagName will be latest commit hash
if [[ "${describe}" =~ ^[A-Fa-f0-9]+$ ]]; then
    echo "No tags found, using defaults"
    major="0"
    minor="0"
    patch="0"
    numericVersion="0.0.0"
    stage=""
    commit_num=`git rev-list HEAD --count`
    #build some magic string to generalize some code below
    commit_num_and_hash="-$commit_num-aaa"
else

    tagName=`git describe --tags --abbrev=0`

    major=`echo $tagName | awk '{split($0,a,"."); print a[1]}'`
    minor=`echo $tagName | awk '{split($0,a,"."); print a[2]}'`
    patch=`echo $tagName | awk '{split($0,a,"."); print a[3]}'`
    numericVersion=`echo $tagName | awk '{split($0,a,"-"); print a[1]}'`
    stage=`echo $tagName | awk '{split($0,a,"-"); print a[2]}'`

    #commit and num will have format like '-3-abc'
    commit_num_and_hash=${describe#$tagName}
    echo "commit and hash part:${commit_num_and_hash}"
fi


if [ "${commit_num_and_hash}" = "" ]; then
    #HEAD commit has tag on it.
    echo "Found tag at HEAD commit"
    fileVersion=${numericVersion}.0
    if [ "${stage}" = "" ]; then
        # just removing empty space from the end and empty dash
        informationVersion="${major}.${minor}"
        version=${numericVersion}
    else
        informationVersion="${major}.${minor} ${stage}"
        version=${numericVersion}-${stage}
    fi
else
    #HEAD commit does not have tag on it
    echo "HEAD commit doesn't have a tag"

    bumpedPatch=$((${patch}+1))
    numericVersion=${major}.${minor}.${bumpedPatch}
    echo "Bumping patch version from ${patch} to ${bumpedPatch}"

    commit_num=`echo $commit_num_and_hash | awk '{split($0,a,"-"); print a[2]}'`
    fileVersion=${numericVersion}.${commit_num}

    if [ "${stage}" = "" ]; then
        stage="preview"
        echo "Auto adding preview stage to version"
    fi

    informationVersion="${major}.${minor} ${stage}"
    version=${numericVersion}-${stage}${commit_num}
fi


echo File version is ${fileVersion}
echo Information version is ${informationVersion}
echo Version is ${version}

