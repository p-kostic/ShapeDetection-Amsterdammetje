![issues](https://img.shields.io/github/issues/p-kostic/ShapeDetection-Amsterdammetje.svg)
![forks](https://img.shields.io/github/forks/p-kostic/ShapeDetection-Amsterdammetje.svg)
![stars](https://img.shields.io/github/stars/p-kostic/ShapeDetection-Amsterdammetje.svg)
![license](	https://img.shields.io/github/license/p-kostic/ShapeDetection-Amsterdammetje.svg)
# Shape Detection: Amsterdammertje
Shape Detection Algorithm for Recognizing those famous Traffic Bollards found in Amsterdam  

Implemented without any libraries other than those provided by C#

## Introduction
An Amsterdammertje is the typical red-brown steel traffic bollard that is used to separate the sidewalk from the street in Amsterdam. Amsterdammertje is Dutch for 'little one from Amsterdam'. The bollards have the three Saint Andrew's Crosses from the coat of arms of Amsterdam.

<p align="center"> 
<img src="https://upload.wikimedia.org/wikipedia/commons/thumb/5/5c/Amsterdammertje.jpg/250px-Amsterdammertje.jpg">
</p>

## Algorithm Description
This program detects waist-high bollards on the street and pavement in images that can be sourced from Google Maps. Given this angle, we are looking for bollards/poles that appear to (or actually do) get narrower towards the top of the image. Note that [Hagenaars](https://nl.wikipedia.org/wiki/Hagenaar_(paaltje)) also fit this criterium. 

### Context Table
| Criterium              | Possible Values                                                                                       |
|------------------------|-------------------------------------------------------------------------------------------------------|
| Minimum / maximum size | More than 140 pixels tall                                                                             |
| Lighting variations   | The poles have to be differing in  intensity from their local background. Both bright and cloudy days |
| Rotation variations    | Between 0.25π and 0.75π in upright position                                                           |
| Occlusion              | Occlusion is acceptable as long as most of the pole's sides are still visible                         |
| Other                  | Pole should get narrower towards the top                                                              |


### Pipeline with Example
**Original Image**  

<p align="center"> 
<img src="https://i.imgur.com/krY97Gz.png">
</p>

**Preprocessing**
* Convert the image to Gray Scale by calculating the average of the colors of that pixel using the following weights:
  ``` C#
  byte average = (byte)(pixelColor.R * 0.299f + pixelColor.B * 0.114f + pixelColor.G * 0.587f);
  ```
  <p align="center">
  <img src="https://i.imgur.com/WtzyHb5.png">
  </p>
  
* Apply a _bilateral filter_ to the gray scaled image to keep edge strength while getting rid of noise.

  <p align="center">
  <img src="https://i.imgur.com/qhqKLrU.png">
  </p>

**Edge Detection**  
Canny edge detection is used to produce an edge map of the previous step, with edges of width `1`. This is to facilitate a faster and more accurate Hough Transform. Our implementation is slightly adapted. Instead of using a gaussian blur before applying sobels, we use a bilateral filter with a fixed `σ = 80`.  The `t`<sub>high</sub> argument is for our Canny Edge detection, which is determined by using Otsu’s method on the grayscale input image.

Example of Canny Edge detection with `q = 85` and `σ = 80`
<p align="center">
  <img src="https://i.imgur.com/lCS4tP3.png">
</p>

**Hough Transform**  
Potential lines were found with angles in range `[0.5π,0.75π]` with a step size of `0.5π / 180`. This ensures only lines that are similar in direction to the sides of the poles are checked for and thus detected. 

<p align="center">
  <img src="https://i.imgur.com/dAYSAHJ.png">
</p>

The red lines in the image above denote the average of the two lines’ mean `x` coordinates. The most likely lines within this range are paired up with their closest partners to find both sides of every pole in the image. 

<p align="center">
  <img src="https://i.imgur.com/WmavVhL.png">
</p>

Pairs that intersect in a positive `y` coordinate are discarded as they do not approach each other to the top, which a pole in our chosen context would.

**Finding the Horizontal Boundaries of the Pole**
* Thicker edges are detected in the original bilaterally filtered gray scale image using 4 Sobel convolution kernels. A threshold based on Otsu’s method with `q = 86` is applied to that image, which results in the following image.

<p align="center">
  <img src="https://i.imgur.com/PWBxynp.png">
</p>

* _Clean outside of pairs_. Pixels outside of the pairs found using the Hough Transform, though with a bit of padding to keep the structure of the previously found edge map, are removed from the image to clean up any remaining noise as seen in the following image.

<p align="center">
  <img src="https://i.imgur.com/gQDDo4O.png">
</p>

* _Object marking_. Objects are marked within the previous step’s result using a flood fill algorithm. The bounding boxes for these found objects are computed and marked, as seen in the following image.

<p align="center">
  <img src="https://i.imgur.com/iEk8ZYO.png">
</p>

* _Object filtering_. Objects that are insufficiently tall and have a width/height ratio that is too wide are filtered, resulting in the following image

<p align="center">
  <img src="https://i.imgur.com/Flr3rwM.png">
</p>

* _Final drawing_. The areas intersected by the bounding boxes in the `y` direction and the paired lines in the `x` direction are colored in on the original image to show the detected poles as seen in the following image.

<p align="center">
  <img src="https://i.imgur.com/EdU7Drl.png">
</p>

## Results of Manual Assessment & Future Work suggestions 
A large collection of images grouped by success is available in the archive and are organized as follows  

* **True positives**. Many of the images resulting in true positives had a clear foreground and background, or a relatively large difference in local contrast between the pole and the pavement. They were often pictures from a somewhat high angle, so the background was solely pavement with no angles of buildings to get in the way. Two exceptions were darkpoles with bright backgrounds, resulting in particularly strong local contrast and thus a high likely hood of edges being properly detected.

* **False Negatives**. Noise is a huge problem. Despite our best efforts (canny edge with Otsu’s method and a bilateral filter) some edges are not strong enough to be detected by our Hough Transform.

* **False Positives & True Negatives**. Finding false positives was actually a challenge. Images of the Eiffel Tower, Bottles, The Washington Monument and deodorant do not get through the flood fill filtering stage. These have been included in the archive. Although they resemble the shape of street poles, intersections and ratio are different enough that they will not be detected. However, any image with a big amount of complexity lines (e.g. animals, hair, fences, etc.) will overload our program when it is trying to find pairs and flood fill. Most likely resulting in a stack overflow of the recursive call. This can be mitigated by introducing limits as a stop condition.

## Acknowledgement
This assignment is a component of the Image Processing course at Utrecht University  

![uulogo](https://www.heilijgers.nl/wp-content/uploads/2018/02/Universiteit-Utrecht-logo.png)

Lecturer: [Dr. ir. R.W. Poppe](https://www.uu.nl/staff/RWPoppe)
