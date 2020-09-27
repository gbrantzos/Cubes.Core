# Release process

- Create branch from develop that contains all features to be released
- Prepare document Changes.vX.XX.XX.md and add it on documents folder
- Commit changes so that CI can prepare a release candidate version
- When finished, merge changes to master, tag with version number so that CI can pick it up and deploy, and finally merge to develop

After release, back on develop: 
- Bump version to version number to be released
