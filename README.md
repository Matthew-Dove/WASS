# WASS
Web Attached SStorage: a place to store all your stuff.  

## Intro

![Wass Overview](assets/images/wass-overview.png)

Like a NAS (*Network Attached Storage*), but with a web focus.  
The extra **S** in WAS**S**, is so the pronunciation is the same as NAS.  
WASS aims to be your backup, and access solution for your private files.  
This project is meant to be consumed by other UIs, such as a console app, website, or a desktop app.  

## TODO

* Backup / Restore file (lock object etc)
* Compress / Decompress file (gzip / brotli)
* Encrypt / Decrypt file
* Add / Remove tag(s) to file (see tags section below)
* Update filename / restore (relative) filepath

### Tags

* Run "command" tags first, then object tags i.e. to delete "delete tags" before the object is removed.
* Option to store tags in a different location - i.e. don't want to read tags from glacier.
* Auto tags for things like: name, extension, size, created, last modified (prefer? since created is reset on file copy), etc.
* Tag alias i.e. mp4 | webm == video, jpg | png == picture

## Steps

A recipe consists of one, or more steps.  
A file will pass though steps as it is sent to a target location.  
For example, you might have a recipe that has the following steps:  
* `FilterFileSizeStep:` LessThan 5GB
* `CompressFileDataStep:` GZip
* `EncryptFileDataStep:` AES256
* `AwsS3StorageStep:` Glacier

This recipe will filter out files >= 5GB in size, compress filedata (_gzip_); encrypt filedata (_aes-256_), then upload to it S3 (_glacier storage_).  
How you use the steps will differ based on the WASS UI (_i.e. console / web / etc_), you can find their descriptions; and ingredients below.  

<details>
<summary>[Step Descriptions]</summary>

### AwsS3StorageStep

Uploads a file to S3.  

Ingredients:  
* `bucket:` the bucket name to upload the file to.
* `storage:` the storage class to use for this file.

</details>

## Philosophy

WASS does not support file deletes.  
WASS does not support file updates.  

If you would like to remove something from storage, you must do so manually by going to your 3rd party storage console, or local drive interfaces.  
In the same vein, if you update a file to "empty", that's the same as deleting / removing it.  

Why? Because if you are backing up a file though WASS, it is important to you; and you do not want to lose it by deleting, or updating local files.  
We want to avoid replicating source file changes to backup target locations.  
By not implementing delete, or update functionity in WASS, it will be _harder_ for WASS to lose your data though any program bugs; or user actions.  
Ideally any api keys you provide WASS contain create-only semantics, as an extra layer of protection.  

WASS treats files as immuable objects, all changes are handled with additions, not overwrites (_a file is identified by it's hash_).  
WASS generally does not use immuable polices provided by 3rd party systems, as they are not fully consistent across the board.  
i.e. your local NAS does not have a data governance policy that WASS can enable with an API call.  
That said there is nothing stopping you from adding them yourself, WASS does not care as long as it still has read; and write permissions.  

WASS does enable some protections to prevent bugs from wiping out data.  
These include things like object versioning in AWS S3, and setting read-only attributes on local files.  

When uploading a file though WASS, firstly we search for the hash at the target server.  
If it exists, we do not upload the file data; but we may still upload metadata (_such as the source path, or search tags_).  

WASS handles metadata the same way as files - immuable & create-only.  
For example when adding _tags_ for a file, we do not have a tags file that is updated over time.  
Instead new files are created under a path stemming from the file's hash value, containing the necessary data.  

As a side note, this philosophy makes WASS a poor choice for backing up things you are currently working on.  
If you are writing a story let's say, each time you saved it; WASS will treat it as a new file (_since the hash has changed_).  
Over time, you might have hundreds of files for your story (_or more!_).  
This gets worse when the work is something larger, for example a photoshop file; or a video that you're editing.  
Therefore, I would not keep any working directories under a target location used by WASS.  

## TODO

* `Wass.Cli` - global dotnet tool to administer WASS though the console.
* `Wass.WebApp` - aws lambda to provide file access over the web, + [Progressive Web App](https://web.dev/add-manifest/) for moblie.
* `Wass.SelfHost` - shares functionality from __Wass.WebApp__, and __Wass.Cli__ to run WASS locally.

![wass-product-diagram](assets/images/wass-product-diagram.png)

## Object Storage

These services have S3 compatible APIs, offer competitive pricing, and are popular community choices:

* [AWS S3](https://aws.amazon.com/s3/)
* [BackBlaze B2](https://www.backblaze.com/cloud-storage)
* [CloudFlare R2](https://www.cloudflare.com/en-au/developer-platform/r2/)
* [Wasabi](https://wasabi.com/)
* [Hetzner Object Storage](https://docs.hetzner.com/storage/object-storage/)
* [DigitalOcean Spaces](https://www.digitalocean.com/products/spaces)
* [IDrive](https://www.idrive.com/s3-storage-e2/)
* [Petabox](https://petabox.io/)
* [Storj](https://www.storj.io/)
* [IceDrive](https://icedrive.net/)

## CDN

When viewing content from WASS (_online_), it's a good idea to use a CDN.  
WASS does not have any native support for CDNs, but using one on top is fine.  
Here are some popular choices:  

* [Bunny](https://bunny.net/cdn/)
* [Cloudflare](https://www.cloudflare.com/cdn/)
* [Fastly](https://www.fastly.com/products/cdn)
* [Keycdn](https://www.keycdn.com/)
* [Cloudfront](https://aws.amazon.com/cloudfront/)

Note that media (_video_) streaming though CDNs is typically sold as a separate / specialised product.  
WASS can pull directly from the source (_without a CDN_), but you may encounter bandwidth limitations; or get charged comical egress traffic fees.  

# Credits

* [Icon](https://www.flaticon.com/free-icon/bird_2630452) made by [Vitaly Gorbachev](https://www.flaticon.com/authors/vitaly-gorbachev) from [Flaticon](https://www.flaticon.com/).

# Changelog

## 0.0.1

* Upload empty project to github.